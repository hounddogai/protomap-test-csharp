namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict succession plan tiers through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictSuccessionPlanTierManager(IStrictSuccessionPlanTierStore Store, StrictSuccessionPlanTierValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict succession plan tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictSuccessionPlanTierStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict succession plan tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictSuccessionPlanTierStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of strict succession plan tier workflow.</summary>
public static class StrictSuccessionPlanTierWorkflow
{
    public static StrictSuccessionPlanTierManager NewStrictSuccessionPlanTierManager(IStrictSuccessionPlanTierStore store, IClock clock)
    {
        return new StrictSuccessionPlanTierManager(
            store,
            StrictSuccessionPlanTierRules.DefaultStrictSuccessionPlanTierValidator(),
            clock,
            new List<string>());
    }
}
