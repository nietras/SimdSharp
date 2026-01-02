using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
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
        int _lineStart = 0;
        int _simdPosition = 0;
        ulong _mask = 0;
        int _currentStart = 0;
        int _currentLength = 0;
        bool _isEnumeratorActive = true;

        internal MaskSpanLineEnumeratorUTF16(ReadOnlySpan<char> span) => _span = span;

        /// <summary>
        /// Gets the line at the current position of the enumerator.
        /// </summary>
        public readonly ReadOnlySpan<char> Current => _span.Slice(_currentStart, _currentLength);

        /// <summary>
        /// Returns this instance as an enumerator.
        /// </summary>
        public readonly MaskSpanLineEnumeratorUTF16 GetEnumerator() => this;

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

            // SIMD path: search for \r or \n using best available vector size
            if (Vector512.IsHardwareAccelerated)
            {
                newlineIndex = SearchWithVector512(span, start);
            }
            else if (Vector256.IsHardwareAccelerated)
            {
                newlineIndex = SearchWithVector256(span, start);
            }
            else if (Vector128.IsHardwareAccelerated)
            {
                newlineIndex = SearchWithVector128(span, start);
            }

            // Scalar fallback: search remaining characters not covered by SIMD using IndexOfAny
            if (newlineIndex == -1)
            {
                var scalarStart = Math.Max(start, _simdPosition);
                var remaining = span.Slice(scalarStart);
                var idx = remaining.IndexOfAny('\n', '\r');
                if (idx >= 0)
                {
                    newlineIndex = scalarStart + idx;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int SearchWithVector512(ReadOnlySpan<char> span, int start)
        {
            var lf = Vector512.Create((ushort)'\n');
            var cr = Vector512.Create((ushort)'\r');

            while (true)
            {
                // First, check if we have any bits left in the current mask
                if (_mask != 0)
                {
                    var bit = BitOperations.TrailingZeroCount(_mask);
                    _mask &= (_mask - 1);

                    // Calculate the absolute position in the span
                    var candidate = (_simdPosition - Vector512<ushort>.Count) + bit;

                    if (candidate >= start)
                    {
                        return candidate;
                    }
                    continue;
                }

                // Try to load and process the next Vector512 chunk
                if (_simdPosition <= span.Length - Vector512<ushort>.Count)
                {
                    var chunk = MemoryMarshal.Cast<char, Vector512<ushort>>(span.Slice(_simdPosition, Vector512<ushort>.Count))[0];
                    var lfs = Vector512.Equals(chunk, lf);
                    var crs = Vector512.Equals(chunk, cr);
                    var matches = Vector512.BitwiseOr(lfs, crs);
                    _mask = Vector512.ExtractMostSignificantBits(matches);
                    _simdPosition += Vector512<ushort>.Count;
                    continue;
                }

                break;
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int SearchWithVector256(ReadOnlySpan<char> span, int start)
        {
            var lf = Vector256.Create((ushort)'\n');
            var cr = Vector256.Create((ushort)'\r');

            while (true)
            {
                // First, check if we have any bits left in the current mask
                if (_mask != 0)
                {
                    var bit = BitOperations.TrailingZeroCount(_mask);
                    _mask &= (_mask - 1);

                    // Calculate the absolute position in the span
                    var candidate = (_simdPosition - Vector256<ushort>.Count) + bit;

                    if (candidate >= start)
                    {
                        return candidate;
                    }
                    continue;
                }

                // Try to load and process the next Vector256 chunk
                if (_simdPosition <= span.Length - Vector256<ushort>.Count)
                {
                    var chunk = MemoryMarshal.Cast<char, Vector256<ushort>>(span.Slice(_simdPosition, Vector256<ushort>.Count))[0];
                    var lfs = Vector256.Equals(chunk, lf);
                    var crs = Vector256.Equals(chunk, cr);
                    var matches = Vector256.BitwiseOr(lfs, crs);
                    _mask = Vector256.ExtractMostSignificantBits(matches);
                    _simdPosition += Vector256<ushort>.Count;
                    continue;
                }

                break;
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int SearchWithVector128(ReadOnlySpan<char> span, int start)
        {
            var lf = Vector128.Create((ushort)'\n');
            var cr = Vector128.Create((ushort)'\r');

            while (true)
            {
                // First, check if we have any bits left in the current mask
                if (_mask != 0)
                {
                    var bit = BitOperations.TrailingZeroCount(_mask);
                    _mask &= (_mask - 1);

                    // Calculate the absolute position in the span
                    var candidate = (_simdPosition - Vector128<ushort>.Count) + bit;

                    if (candidate >= start)
                    {
                        return candidate;
                    }
                    continue;
                }

                // Try to load and process the next Vector128 chunk
                if (_simdPosition <= span.Length - Vector128<ushort>.Count)
                {
                    var chunk = MemoryMarshal.Cast<char, Vector128<ushort>>(span.Slice(_simdPosition, Vector128<ushort>.Count))[0];
                    var lfs = Vector128.Equals(chunk, lf);
                    var crs = Vector128.Equals(chunk, cr);
                    var matches = Vector128.BitwiseOr(lfs, crs);
                    _mask = Vector128.ExtractMostSignificantBits(matches);
                    _simdPosition += Vector128<ushort>.Count;
                    continue;
                }

                break;
            }

            return -1;
        }

        /// <inheritdoc />
        readonly object IEnumerator.Current => throw new NotSupportedException();

        /// <inheritdoc />
        readonly void IEnumerator.Reset() => throw new NotSupportedException();

        /// <inheritdoc />
        readonly void IDisposable.Dispose() { }
    }
}
