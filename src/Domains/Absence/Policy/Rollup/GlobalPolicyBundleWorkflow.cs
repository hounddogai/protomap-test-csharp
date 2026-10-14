namespace Acme.Workforce.Absence.Policy.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global policy bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalPolicyBundleWorkflow(IGlobalPolicyBundleStore Store, GlobalPolicyBundleValidator Validator, IClock Clock, List<string> Journal)
{
    public static GlobalPolicyBundleWorkflow NewGlobalPolicyBundleWorkflow(IGlobalPolicyBundleStore store, IClock clock)
    {
        return new GlobalPolicyBundleWorkflow(
            store,
            GlobalPolicyBundleRules.DefaultGlobalPolicyBundleValidator(),
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
            throw new InvalidOperationException($"global policy bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalPolicyBundleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global policy bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalPolicyBundleStatus.Closed);
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
            if (item.StartsAt > cutoff)
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
