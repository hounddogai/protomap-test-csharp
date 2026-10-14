namespace Acme.Workforce.Contractors.Offboarding.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves offboarding slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record OffboardingSlotCoordinator(IOffboardingSlotStore Store, OffboardingSlotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OffboardingSlotStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OffboardingSlotStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public int Sweep(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}

/// <summary>Functions of offboarding slot workflow.</summary>
public static class OffboardingSlotWorkflow
{
    public static OffboardingSlotCoordinator NewOffboardingSlotCoordinator(IOffboardingSlotStore store, IClock clock)
    {
        return new OffboardingSlotCoordinator(
            store,
            OffboardingSlotRules.DefaultOffboardingSlotValidator(),
            clock,
            new List<string>());
    }
}
