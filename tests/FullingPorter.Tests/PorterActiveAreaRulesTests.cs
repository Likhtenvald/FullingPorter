using FullingPorter.Core;
using Xunit;

namespace FullingPorter.Tests;

public sealed class PorterActiveAreaRulesTests
{
    [Fact]
    public void EmptyBaseRemainsActiveWhenHostIsAway()
    {
        Assert.True(PorterActiveAreaRules.NeedsServerArea(
            homeX: 0f, homeZ: 0f,
            hostX: 250f, hostZ: 0f,
            activeRadius: 64f));
    }

    [Fact]
    public void HostOutsideActiveAreaNeedsServerArea()
    {
        Assert.True(PorterActiveAreaRules.NeedsServerArea(
            homeX: 0f, homeZ: 0f,
            hostX: 65f, hostZ: 0f,
            activeRadius: 64f));
    }

    [Fact]
    public void HostAtBaseUsesVanillaActiveArea()
    {
        Assert.False(PorterActiveAreaRules.NeedsServerArea(
            homeX: 0f, homeZ: 0f,
            hostX: 4f, hostZ: 0f,
            activeRadius: 64f));
    }

    [Fact]
    public void HostAtActiveAreaBoundaryUsesVanillaActiveArea()
    {
        Assert.False(PorterActiveAreaRules.NeedsServerArea(
            homeX: 0f, homeZ: 0f,
            hostX: 64f, hostZ: 0f,
            activeRadius: 64f));
    }
}
