namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal offboarding tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalOffboardingTierManager(ISeasonalOffboardingTierStore Store, SeasonalOffboardingTierValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal offboarding tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalOffboardingTierStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal offboarding tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalOffboardingTierStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of seasonal offboarding tier workflow.</summary>
public static class SeasonalOffboardingTierWorkflow
{
    public static SeasonalOffboardingTierManager NewSeasonalOffboardingTierManager(ISeasonalOffboardingTierStore store, IClock clock)
    {
        return new SeasonalOffboardingTierManager(
            store,
            SeasonalOffboardingTierRules.DefaultSeasonalOffboardingTierValidator(),
            clock,
            new List<string>());
    }
}
