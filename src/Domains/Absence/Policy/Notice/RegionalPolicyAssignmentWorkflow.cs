namespace Acme.Workforce.Absence.Policy.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional policy assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalPolicyAssignmentCoordinator(IRegionalPolicyAssignmentStore Store, RegionalPolicyAssignmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional policy assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalPolicyAssignmentStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional policy assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalPolicyAssignmentStatus.Withdrawn);
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

/// <summary>Functions of regional policy assignment workflow.</summary>
public static class RegionalPolicyAssignmentWorkflow
{
    public static RegionalPolicyAssignmentCoordinator NewRegionalPolicyAssignmentCoordinator(IRegionalPolicyAssignmentStore store, IClock clock)
    {
        return new RegionalPolicyAssignmentCoordinator(
            store,
            RegionalPolicyAssignmentRules.DefaultRegionalPolicyAssignmentValidator(),
            clock,
            new List<string>());
    }
}
