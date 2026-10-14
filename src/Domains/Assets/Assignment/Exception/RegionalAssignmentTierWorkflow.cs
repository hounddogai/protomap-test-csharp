namespace Acme.Workforce.Assets.Assignment.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional assignment tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalAssignmentTierCoordinator(IRegionalAssignmentTierStore Store, RegionalAssignmentTierValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional assignment tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalAssignmentTierStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional assignment tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalAssignmentTierStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of regional assignment tier workflow.</summary>
public static class RegionalAssignmentTierWorkflow
{
    public static RegionalAssignmentTierCoordinator NewRegionalAssignmentTierCoordinator(IRegionalAssignmentTierStore store, IClock clock)
    {
        return new RegionalAssignmentTierCoordinator(
            store,
            RegionalAssignmentTierRules.DefaultRegionalAssignmentTierValidator(),
            clock,
            new List<string>());
    }
}
