// SPDX-License-Identifier: AGPL-3.0-only
using System.Text;
using Wutong;
using Xunit;

public class WtpTests
{
    [Fact]
    public void CanonicalizeSortsKeys() =>
        Assert.Equal("{\"a\":\"x\",\"b\":1}", Wtp.Canonicalize("{\"b\":1,\"a\":\"x\"}"));

    [Theory]
    [InlineData("{\"a\":1,\"a\":2}")]
    [InlineData("{\"x\":1.2}")]
    [InlineData("{\"x\":NaN}")]
    [InlineData("{\"x\":9007199254740992}")]
    public void StrictJsonRejected(string bad) =>
        Assert.Equal("ERR_SCHEMA", Assert.Throws<WtpException>(() => Wtp.Canonicalize(bad)).Code);

    [Fact]
    public void Sha256Vector() =>
        Assert.Equal("sha256:ed8a6d893471ec2a4895c1acc601904f65d5416d16f15f1c1fd6e221d0bf875e",
            Wtp.Digest(Encoding.UTF8.GetBytes("Hello, Wutong")));

    [Fact]
    public void ValidatesPrincipal() => Wtp.Validate(Encoding.UTF8.GetBytes(
        "{\"protocol\":\"wutong/0.1\",\"kind\":\"wutong.principal\",\"id\":\"urn:uuid:019a1111-1111-7111-8111-000000000064\",\"occurred_at\":\"2026-10-18T10:00:00Z\",\"payload\":{\"principal_id\":\"urn:uuid:019a1111-1111-7111-8111-000000000065\",\"principal_type\":\"agent\"}}"));

    [Fact]
    public void RejectsIncompleteEnvelope() =>
        Assert.StartsWith("ERR_", Assert.Throws<WtpException>(() => Wtp.Validate("{}"u8)).Code);

    [Fact]
    public void ReportsImplementation() => Assert.StartsWith("wutong-core-rs/", Wtp.Implementation);
}
