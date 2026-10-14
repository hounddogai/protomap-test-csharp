namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal offboarding slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalOffboardingSlotWorkflow(ISeasonalOffboardingSlotStore Store, SeasonalOffboardingSlotValidator Validator, IClock Clock, List<string> Journal)
{
    public static SeasonalOffboardingSlotWorkflow NewSeasonalOffboardingSlotWorkflow(ISeasonalOffboardingSlotStore store, IClock clock)
    {
        return new SeasonalOffboardingSlotWorkflow(
            store,
            SeasonalOffboardingSlotRules.DefaultSeasonalOffboardingSlotValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal offboarding slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalOffboardingSlotStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal offboarding slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalOffboardingSlotStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
