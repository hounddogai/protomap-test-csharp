namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict assignment baselines through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictAssignmentBaselineCoordinator(IStrictAssignmentBaselineStore Store, StrictAssignmentBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict assignment baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictAssignmentBaselineStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict assignment baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictAssignmentBaselineStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of strict assignment baseline workflow.</summary>
public static class StrictAssignmentBaselineWorkflow
{
    public static StrictAssignmentBaselineCoordinator NewStrictAssignmentBaselineCoordinator(IStrictAssignmentBaselineStore store, IClock clock)
    {
        return new StrictAssignmentBaselineCoordinator(
            store,
            StrictAssignmentBaselineRules.DefaultStrictAssignmentBaselineValidator(),
            clock,
            new List<string>());
    }
}
