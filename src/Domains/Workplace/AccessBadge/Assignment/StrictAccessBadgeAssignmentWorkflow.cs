namespace Acme.Workforce.Workplace.AccessBadge.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict access badge assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictAccessBadgeAssignmentService(IStrictAccessBadgeAssignmentStore Store, StrictAccessBadgeAssignmentValidator Validator, IClock Clock, List<string> Journal, StrictAccessBadgeAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict access badge assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictAccessBadgeAssignmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict access badge assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictAccessBadgeAssignmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict access badge assignment {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of strict access badge assignment workflow.</summary>
public static class StrictAccessBadgeAssignmentWorkflow
{
    public static StrictAccessBadgeAssignmentService NewStrictAccessBadgeAssignmentService(IStrictAccessBadgeAssignmentStore store, IClock clock)
    {
        return new StrictAccessBadgeAssignmentService(
            store,
            StrictAccessBadgeAssignmentRules.DefaultStrictAccessBadgeAssignmentValidator(),
            clock,
            new List<string>(),
            StrictAccessBadgeAssignmentEngine.DefaultStrictAccessBadgeAssignmentEngine());
    }
}
