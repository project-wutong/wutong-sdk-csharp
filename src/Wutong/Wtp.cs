// SPDX-License-Identifier: AGPL-3.0-only
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Wutong;

/// <summary>A Wutong Protocol error carrying the normative ERR_* code.</summary>
public sealed class WtpException : Exception
{
    /// <summary>Normative error code, e.g. <c>ERR_SCHEMA</c>.</summary>
    public string Code { get; }

    /// <summary>Create from a core message of the form <c>ERR_X: detail</c>.</summary>
    public WtpException(string message) : base(message)
    {
        var i = message.IndexOf(':');
        Code = i > 0 ? message[..i] : "ERR_INTERNAL";
    }
}

internal static unsafe partial class Native
{
    private const string Lib = "wutong_ffi";

    [LibraryImport(Lib, EntryPoint = "wutong_abi_version")]
    internal static partial uint AbiVersion();

    [LibraryImport(Lib, EntryPoint = "wutong_implementation")]
    internal static partial IntPtr Implementation();

    [LibraryImport(Lib, EntryPoint = "wutong_jcs")]
    internal static partial int Jcs(byte* json, nuint len, out IntPtr output, out IntPtr error);

    [LibraryImport(Lib, EntryPoint = "wutong_validate_envelope")]
    internal static partial int Validate(byte* json, nuint len, out IntPtr error);

    [LibraryImport(Lib, EntryPoint = "wutong_sha256")]
    internal static partial IntPtr Sha256(byte* data, nuint len);

    [LibraryImport(Lib, EntryPoint = "wutong_string_free")]
    internal static partial void Free(IntPtr s);
}

/// <summary>Wutong Protocol operations. All rules are implemented once, in the Rust core.</summary>
public static unsafe class Wtp
{
    /// <summary>Protocol identifier.</summary>
    public const string Protocol = "wutong/0.1";

    /// <summary>ABI version of the loaded native core.</summary>
    public static uint AbiVersion => Native.AbiVersion();

    /// <summary>Implementation string of the native core.</summary>
    public static string Implementation => Marshal.PtrToStringUTF8(Native.Implementation()) ?? "";

    private static string Take(IntPtr p)
    {
        try { return Marshal.PtrToStringUTF8(p) ?? ""; }
        finally { Native.Free(p); }
    }

    /// <summary>Strictly parse JSON and return its RFC 8785 canonical form.</summary>
    public static string Canonicalize(ReadOnlySpan<byte> json)
    {
        fixed (byte* p = json)
        {
            if (Native.Jcs(p, (nuint)json.Length, out var o, out var e) != 0) throw new WtpException(Take(e));
            return Take(o);
        }
    }

    /// <summary>Canonicalize a JSON string.</summary>
    public static string Canonicalize(string json) => Canonicalize(Encoding.UTF8.GetBytes(json));

    /// <summary>Strict parse + schema + semantic validation; throws <see cref="WtpException"/>.</summary>
    public static void Validate(ReadOnlySpan<byte> json)
    {
        fixed (byte* p = json)
        {
            if (Native.Validate(p, (nuint)json.Length, out var e) != 0) throw new WtpException(Take(e));
        }
    }

    /// <summary><c>sha256:&lt;hex&gt;</c> of the exact bytes.</summary>
    public static string Digest(ReadOnlySpan<byte> data)
    {
        fixed (byte* p = data) return Take(Native.Sha256(p, (nuint)data.Length));
    }
}
