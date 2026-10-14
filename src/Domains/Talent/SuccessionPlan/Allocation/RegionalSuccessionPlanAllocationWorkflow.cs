namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional succession plan allocations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalSuccessionPlanAllocationManager(IRegionalSuccessionPlanAllocationStore Store, RegionalSuccessionPlanAllocationValidator Validator, IClock Clock, List<string> Journal, RegionalSuccessionPlanAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional succession plan allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalSuccessionPlanAllocationStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional succession plan allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalSuccessionPlanAllocationStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional succession plan allocation {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of regional succession plan allocation workflow.</summary>
public static class RegionalSuccessionPlanAllocationWorkflow
{
    public static RegionalSuccessionPlanAllocationManager NewRegionalSuccessionPlanAllocationManager(IRegionalSuccessionPlanAllocationStore store, IClock clock, Registry flags)
    {
        return new RegionalSuccessionPlanAllocationManager(
            store,
            RegionalSuccessionPlanAllocationRules.DefaultRegionalSuccessionPlanAllocationValidator(),
            clock,
            new List<string>(),
            RegionalSuccessionPlanAllocationEngine.DefaultRegionalSuccessionPlanAllocationEngine(flags));
    }
}
