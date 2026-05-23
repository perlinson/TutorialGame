using NUnit.Framework;

public sealed class ExpeditionEventResolverEditModeTests
{
    [Test]
    public void Resolve_PrefersHigherPriorityCandidatesBeforeWeight()
    {
        var resolver = new ExpeditionEventResolver();
        var definitions = new[]
        {
            new ExpeditionEventDefinition
            {
                Id = "generic_heavy",
                CardType = ExpeditionEventCardType.Generic,
                Weight = 999
            },
            new ExpeditionEventDefinition
            {
                Id = "task_injected",
                CardType = ExpeditionEventCardType.TaskInjected,
                Weight = 1
            }
        };

        var result = resolver.Resolve(definitions, new ExpeditionEventRuntimeContext(), (_, __) => true, 42);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("task_injected"));
    }

    [Test]
    public void Resolve_UsesExplicitPriorityOverrideWhenConfigured()
    {
        var resolver = new ExpeditionEventResolver();
        var definitions = new[]
        {
            new ExpeditionEventDefinition
            {
                Id = "generic_priority_override",
                CardType = ExpeditionEventCardType.Generic,
                Priority = 500,
                Weight = 1
            },
            new ExpeditionEventDefinition
            {
                Id = "task_default_priority",
                CardType = ExpeditionEventCardType.TaskInjected,
                Weight = 1
            }
        };

        var result = resolver.Resolve(definitions, new ExpeditionEventRuntimeContext(), (_, __) => true, 42);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("generic_priority_override"));
    }

    [Test]
    public void Resolve_ReturnsNullWhenNoCandidatesMatch()
    {
        var resolver = new ExpeditionEventResolver();
        var definitions = new[]
        {
            new ExpeditionEventDefinition
            {
                Id = "generic_event",
                CardType = ExpeditionEventCardType.Generic,
                Weight = 1
            }
        };

        var result = resolver.Resolve(definitions, new ExpeditionEventRuntimeContext(), (_, __) => false, 42);

        Assert.That(result, Is.Null);
    }
}
