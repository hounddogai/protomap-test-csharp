namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves assignment baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssignmentBaselineCoordinator(IAssignmentBaselineStore Store, AssignmentBaselineValidator Validator, IClock Clock, List<string> Journal, AssignmentBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssignmentBaselineStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssignmentBaselineStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment baseline {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of assignment baseline workflow.</summary>
public static class AssignmentBaselineWorkflow
{
    public static AssignmentBaselineCoordinator NewAssignmentBaselineCoordinator(IAssignmentBaselineStore store, IClock clock)
    {
        return new AssignmentBaselineCoordinator(
            store,
            AssignmentBaselineRules.DefaultAssignmentBaselineValidator(),
            clock,
            new List<string>(),
            AssignmentBaselineEngine.DefaultAssignmentBaselineEngine());
    }
}
