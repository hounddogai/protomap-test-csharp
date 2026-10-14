namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered policy baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredPolicyBaselineManager(ITieredPolicyBaselineStore Store, TieredPolicyBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered policy baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredPolicyBaselineStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered policy baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredPolicyBaselineStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of tiered policy baseline workflow.</summary>
public static class TieredPolicyBaselineWorkflow
{
    public static TieredPolicyBaselineManager NewTieredPolicyBaselineManager(ITieredPolicyBaselineStore store, IClock clock)
    {
        return new TieredPolicyBaselineManager(
            store,
            TieredPolicyBaselineRules.DefaultTieredPolicyBaselineValidator(),
            clock,
            new List<string>());
    }
}
