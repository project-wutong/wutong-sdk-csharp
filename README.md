# Wutong SDK for .NET · 梧桐 C# SDK

[![CI](https://github.com/project-wutong/wutong-sdk-csharp/actions/workflows/ci.yml/badge.svg)](https://github.com/project-wutong/wutong-sdk-csharp/actions/workflows/ci.yml)
.NET 8 · P/Invoke (`LibraryImport`) over the Rust core's C ABI · `AGPL-3.0-only`

## 中文

C# SDK 通过 `LibraryImport` 调用 Rust 核心导出的 C ABI（`wutong_ffi`），C# 层不实现协议规则。
错误以 `WtpException`（`Code` 为规范错误码）抛出。NuGet 包将随各平台原生库（runtimes/*/native）在后续里程碑发布。

```csharp
using Wutong;
var canon = Wtp.Canonicalize("{\"b\":1,\"a\":2}");
Wtp.Validate(File.ReadAllBytes("message.json"));
```

本地运行测试：先在 `wutong-core` 中 `cargo build --release -p wutong-ffi`，再
`LD_LIBRARY_PATH=../wutong-core/target/release dotnet test`。

## English

Thin P/Invoke wrapper over the stable C ABI exported by `wutong-core/crates/wutong-ffi`.
Native packaging per RID is planned.
