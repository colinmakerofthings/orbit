using FluentAssertions;
using Orbit.Core.Models;
using Orbit.Engine.Engine;

namespace Orbit.UnitTests;

public class CommandRegistryTests
{
    [Fact]
    public void Register_And_Find_ReturnsCommand()
    {
        var registry = new CommandRegistry();
        var cmd = new OrbitCommand { Name = "start-rancher", WorkflowName = "rancher-start" };

        registry.Register(cmd);
        var found = registry.Find("start-rancher");

        found.Should().NotBeNull();
        found!.WorkflowName.Should().Be("rancher-start");
    }

    [Fact]
    public void Find_IsCaseInsensitive()
    {
        var registry = new CommandRegistry();
        registry.Register(new OrbitCommand { Name = "start-rancher", WorkflowName = "rancher-start" });

        registry.Find("START-RANCHER").Should().NotBeNull();
        registry.Find("Start-Rancher").Should().NotBeNull();
    }

    [Fact]
    public void Find_UnknownCommand_ReturnsNull()
    {
        var registry = new CommandRegistry();
        registry.Find("ghost").Should().BeNull();
    }

    [Fact]
    public void GetAll_ReturnsAllRegistered()
    {
        var registry = new CommandRegistry();
        registry.Register(new OrbitCommand { Name = "a", WorkflowName = "wf-a" });
        registry.Register(new OrbitCommand { Name = "b", WorkflowName = "wf-b" });

        registry.GetAll().Should().HaveCount(2);
    }

    [Fact]
    public void Register_SameName_Overwrites()
    {
        var registry = new CommandRegistry();
        registry.Register(new OrbitCommand { Name = "cmd", WorkflowName = "first" });
        registry.Register(new OrbitCommand { Name = "cmd", WorkflowName = "second" });

        registry.Find("cmd")!.WorkflowName.Should().Be("second");
    }
}
