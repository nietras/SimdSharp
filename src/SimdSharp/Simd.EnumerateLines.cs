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
        int _lineStart = 0;
        int _position = 0;
        long _mask = 0;
        int _currentStart = 0;
        int _currentLength = 0;

        internal MaskSpanLineEnumeratorUTF16(ReadOnlySpan<char> span)
        {
            _span = span;
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
            var span = _span;
            if (_lineStart >= span.Length)
            {
                _currentLength = 0;
                return false;
            }

            var start = _lineStart;
            var newlineIndex = -1;

            while (true)
            {
                if (_mask != 0)
                {
                    var bit = BitOperations.TrailingZeroCount((ulong)_mask);
                    newlineIndex = (_position - Vector512<ushort>.Count) + bit;
                    _mask &= ~(1L << bit);
                    break;
                }

                var lf = Vector512.Create((ushort)'\n');
                var cr = Vector512.Create((ushort)'\r');
                if (_position <= span.Length - Vector512<ushort>.Count)
                {
                    var chunk = MemoryMarshal.Cast<char, Vector512<ushort>>(span.Slice(_position, Vector512<ushort>.Count))[0];
                    var lfs = Vector512.Equals(chunk, lf);
                    var crs = Vector512.Equals(chunk, cr);
                    var matches = Vector512.BitwiseOr(lfs, crs);
                    _mask = (long)Vector512.ExtractMostSignificantBits(matches);
                    _position += Vector512<ushort>.Count;

                    if (_mask != 0)
                    {
                        continue;
                    }

                    continue;
                }

                break;
            }

            if (newlineIndex == -1)
            {
                for (; _position < span.Length; _position++)
                {
                    var ch = span[_position];
                    if (ch == '\n' || ch == '\r')
                    {
                        newlineIndex = _position;
                        _position++;
                        break;
                    }
                }
            }

            if (newlineIndex == -1)
            {
                if (start >= span.Length)
                {
                    _lineStart = span.Length + 1;
                    _currentLength = 0;
                    return false;
                }

                _currentStart = start;
                _currentLength = span.Length - start;
                _lineStart = span.Length + 1;
                _mask = 0;
                _position = span.Length;
                return true;
            }

            var stride = 1;
            var newlineChar = span[newlineIndex];
            if (newlineChar == '\r' && newlineIndex + 1 < span.Length && span[newlineIndex + 1] == '\n')
            {
                stride = 2;
            }

            _currentStart = start;
            _currentLength = newlineIndex - start;
            _lineStart = newlineIndex + stride;

            if (_lineStart > _position)
            {
                _position = _lineStart;
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
