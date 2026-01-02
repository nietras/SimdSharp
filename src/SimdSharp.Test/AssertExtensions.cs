using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimdSharp.Test;

public static class AssertEx
{
    [return: NotNullIfNotNull(nameof(actual))]
    public static T? AreEqualReturn<T>(T? expected, T? actual, string? message = "",
        [CallerArgumentExpression(nameof(expected))] string expectedExpression = "",
        [CallerArgumentExpression(nameof(actual))] string actualExpression = "")
    {
        Assert.AreEqual(expected, actual, message, expectedExpression, actualExpression);
        return actual;
    }
}
