using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace SimdSharp;

public static partial class Simd
{
    public static MaskSpanLineEnumeratorUTF16 EnumerateLines(ReadOnlySpan<char> span)
        => new(span);

    /// <summary>
    /// Enumerates the lines of a <see cref="ReadOnlySpan{Char}"/>.
    /// </summary>
    /// <remarks>
    /// To get an instance of this type, use <see cref="Simd.EnumerateLines(ReadOnlySpan{char})"/>.
    /// </remarks>
    public ref struct MaskSpanLineEnumeratorUTF16 : IEnumerator<ReadOnlySpan<char>>
    {
        readonly ReadOnlySpan<char> _span;
        int _lineStart;
        int _simdPosition;
        long _mask;
        int _currentStart;
        int _currentLength;
        bool _isEnumeratorActive;

        internal MaskSpanLineEnumeratorUTF16(ReadOnlySpan<char> span)
        {
            _span = span;
            _lineStart = 0;
            _simdPosition = 0;
            _mask = 0;
            _currentStart = 0;
            _currentLength = 0;
            _isEnumeratorActive = true;
        }

        /// <summary>
        /// Gets the line at the current position of the enumerator.
        /// </summary>
        public ReadOnlySpan<char> Current => _span.Slice(_currentStart, _currentLength);

        /// <summary>
        /// Returns this instance as an enumerator.
        /// </summary>
        public MaskSpanLineEnumeratorUTF16 GetEnumerator() => this;

        /// <summary>
        /// Advances the enumerator to the next line of the span.
        /// </summary>
        /// <returns>
        /// True if the enumerator successfully advanced to the next line; false if
        /// the enumerator has advanced past the end of the span.
        /// </returns>
        public bool MoveNext()
        {
            if (!_isEnumeratorActive)
            {
                _currentStart = 0;
                _currentLength = 0;
                return false;
            }

            var span = _span;
            var start = _lineStart;
            var newlineIndex = -1;

            // SIMD path: search for \r or \n using Vector512
            while (true)
            {
                // First, check if we have any bits left in the current mask
                if (_mask != 0)
                {
                    var bit = BitOperations.TrailingZeroCount((ulong)_mask);
                    _mask &= ~(1L << bit);

                    // Calculate the absolute position in the span
                    // The mask corresponds to the chunk that ended at _simdPosition
                    var candidate = (_simdPosition - Vector512<ushort>.Count) + bit;

                    if (candidate >= start)
                    {
                        newlineIndex = candidate;
                        break;
                    }
                    continue;
                }

                // Try to load and process the next Vector512 chunk
                if (_simdPosition <= span.Length - Vector512<ushort>.Count)
                {
                    var lf = Vector512.Create((ushort)'\n');
                    var cr = Vector512.Create((ushort)'\r');

                    var chunk = MemoryMarshal.Cast<char, Vector512<ushort>>(span.Slice(_simdPosition, Vector512<ushort>.Count))[0];
                    var lfs = Vector512.Equals(chunk, lf);
                    var crs = Vector512.Equals(chunk, cr);
                    var matches = Vector512.BitwiseOr(lfs, crs);
                    _mask = (long)Vector512.ExtractMostSignificantBits(matches);
                    _simdPosition += Vector512<ushort>.Count;

                    continue;
                }

                // No more full vectors to process, exit SIMD loop
                break;
            }

            // Scalar fallback: search remaining characters not covered by SIMD
            if (newlineIndex == -1)
            {
                // Search from where SIMD left off (or from start if no SIMD processing occurred)
                var scalarStart = Math.Max(start, _simdPosition);
                for (var i = scalarStart; i < span.Length; i++)
                {
                    var ch = span[i];
                    if (ch == '\n' || ch == '\r')
                    {
                        newlineIndex = i;
                        break;
                    }
                }
            }

            if (newlineIndex >= 0)
            {
                var stride = 1;

                if (span[newlineIndex] == '\r' &&
                    (uint)(newlineIndex + 1) < (uint)span.Length &&
                    span[newlineIndex + 1] == '\n')
                {
                    stride = 2;
                }

                _currentStart = start;
                _currentLength = newlineIndex - start;
                _lineStart = newlineIndex + stride;
            }
            else
            {
                // No more newlines found - return the final segment
                _currentStart = start;
                _currentLength = span.Length - start;
                _isEnumeratorActive = false;
            }

            return true;
        }

        /// <inheritdoc />
        object IEnumerator.Current => throw new NotSupportedException();

        /// <inheritdoc />
        void IEnumerator.Reset() => throw new NotSupportedException();

        /// <inheritdoc />
        void IDisposable.Dispose() { }
    }
}
