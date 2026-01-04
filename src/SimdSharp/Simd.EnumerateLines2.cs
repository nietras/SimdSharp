using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace SimdSharp;

public static partial class Simd
{
    public static MaskSpanLineEnumeratorUTF16New EnumerateLinesNew(ReadOnlySpan<char> span)
        => new(span);

    /// <summary>
    /// Enumerates the lines of a <see cref="ReadOnlySpan{Char}"/>.
    /// </summary>
    /// <remarks>
    /// To get an instance of this type, use <see cref="Simd.EnumerateLines(ReadOnlySpan{char})"/>.
    /// </remarks>
    public ref struct MaskSpanLineEnumeratorUTF16New : IEnumerator<ReadOnlySpan<char>>
    {
        readonly ReadOnlySpan<char> _span;
        int _lineStart = 0;
        int _searchPosition = 0;  // Next position to search from (advances after each vector is fully processed)
        int _maskBasePosition = 0;  // Base position for current non-zero mask
        ulong _mask = 0;
        int _currentStart = 0;
        int _currentLength = 0;
        bool _isEnumeratorActive = true;

        internal MaskSpanLineEnumeratorUTF16New(ReadOnlySpan<char> span) => _span = span;

        /// <summary>
        /// Gets the line at the current position of the enumerator.
        /// </summary>
        public readonly ReadOnlySpan<char> Current => _span.Slice(_currentStart, _currentLength);

        /// <summary>
        /// Returns this instance as an enumerator.
        /// </summary>
        public readonly MaskSpanLineEnumeratorUTF16New GetEnumerator() => this;

        /// <summary>
        /// Advances the enumerator to the next line of the span.
        /// </summary>
        /// <returns>
        /// True if the enumerator successfully advanced to the next line; false if
        /// the enumerator has advanced past the end of the span.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            var span = _span;
        MASK:
            if (_mask != 0)
            {
                var bit = BitOperations.TrailingZeroCount(_mask);
                _mask &= (_mask - 1);

                var candidate = _maskBasePosition + bit;
                Debug.Assert(candidate >= _lineStart);
                var stride = 1;
                if (span[candidate] == '\r' &&
                    (uint)(candidate + 1) < (uint)span.Length &&
                    span[candidate + 1] == '\n')
                {
                    stride = 2;
                    // Clear the \n bit from mask if it's in the mask, which
                    // it is if mask is still not zero, after bit clear.
                    if (_mask != 0)
                    {
                        _mask &= (_mask - 1);
                    }
                    if (candidate + 1 == _searchPosition)
                    {
                        ++_searchPosition;
                    }
                }
                _currentStart = _lineStart;
                _currentLength = candidate - _lineStart;
                _lineStart = candidate + stride;
                return true;
            }

            if (!_isEnumeratorActive)
            {
                _currentStart = 0;
                _currentLength = 0;
                return false;
            }
            Debug.Assert(_mask == 0);
            _mask = SearchNextMask(span);
            if (_mask != 0)
            {
                goto MASK;
            }

            // No more newlines found - return the final segment
            _currentStart = _lineStart;
            _currentLength = span.Length - _lineStart;
            _mask = 0;
            _isEnumeratorActive = false;
            return true;
        }

        ulong SearchNextMask(ReadOnlySpan<char> span)
        {
            ulong mask = 0;
            if (Vector512.IsHardwareAccelerated)
            {
                mask = SearchMask512(span);
            }
            else if (Vector256.IsHardwareAccelerated)
            {
                mask = SearchMask256(span);
            }
            else if (Vector128.IsHardwareAccelerated)
            {
                mask = SearchMask128(span);
            }
            if (mask == 0)
            {
                var scalarStart = Math.Max(_lineStart, _searchPosition);
                for (var i = scalarStart; i < span.Length; i++)
                {
                    var c = span[i];
                    var lf = c == '\n' ? 1 : 0;
                    var cr = c == '\r' ? 1 : 0;
                    var m = lf | cr;
                    if (m != 0)
                    {
                        mask = 1UL;
                        _maskBasePosition = i;
                        break;
                    }
                }
            }
            return mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ulong SearchMask512(ReadOnlySpan<char> span)
        {
            var lf = Vector512.Create((ushort)'\n');
            var cr = Vector512.Create((ushort)'\r');

            ulong mask = 0;
            var searchPosition = _searchPosition;
            var maskBasePosition = 0;
            ref var spanRef = ref Unsafe.As<char, ushort>(ref MemoryMarshal.GetReference(span));
            var end = span.Length - Vector512<ushort>.Count;
            while (mask == 0 && searchPosition <= end)
            {
                maskBasePosition = searchPosition;
                ref var pos = ref Unsafe.Add(ref spanRef, searchPosition);
                var chunk = Vector512.LoadUnsafe(ref pos);
                var lfs = Vector512.Equals(chunk, lf);
                var crs = Vector512.Equals(chunk, cr);
                var matches = Vector512.BitwiseOr(lfs, crs);
                mask = Vector512.ExtractMostSignificantBits(matches);
                searchPosition += Vector512<ushort>.Count;
            }
            _searchPosition = searchPosition;
            _maskBasePosition = maskBasePosition;
            return mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ulong SearchMask256(ReadOnlySpan<char> span)
        {
            var lf = Vector256.Create((ushort)'\n');
            var cr = Vector256.Create((ushort)'\r');

            ulong mask = 0;
            while (mask == 0 && _searchPosition <= span.Length - Vector256<ushort>.Count)
            {
                _maskBasePosition = _searchPosition;
                var chunk = MemoryMarshal.Cast<char, Vector256<ushort>>(span.Slice(_searchPosition, Vector256<ushort>.Count))[0];
                var lfs = Vector256.Equals(chunk, lf);
                var crs = Vector256.Equals(chunk, cr);
                var matches = Vector256.BitwiseOr(lfs, crs);
                mask = Vector256.ExtractMostSignificantBits(matches);
                _searchPosition += Vector256<ushort>.Count;
            }
            return mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ulong SearchMask128(ReadOnlySpan<char> span)
        {
            var lf = Vector128.Create((ushort)'\n');
            var cr = Vector128.Create((ushort)'\r');

            ulong mask = 0;
            while (mask == 0 && _searchPosition <= span.Length - Vector128<ushort>.Count)
            {
                _maskBasePosition = _searchPosition;
                var chunk = MemoryMarshal.Cast<char, Vector128<ushort>>(span.Slice(_searchPosition, Vector128<ushort>.Count))[0];
                var lfs = Vector128.Equals(chunk, lf);
                var crs = Vector128.Equals(chunk, cr);
                var matches = Vector128.BitwiseOr(lfs, crs);
                mask = Vector128.ExtractMostSignificantBits(matches);
                _searchPosition += Vector128<ushort>.Count;
            }
            return mask;
        }

        /// <inheritdoc />
        readonly object IEnumerator.Current => throw new NotSupportedException();

        /// <inheritdoc />
        readonly void IEnumerator.Reset() => throw new NotSupportedException();

        /// <inheritdoc />
        readonly void IDisposable.Dispose() { }
    }
}
