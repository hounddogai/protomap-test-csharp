namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global engagement assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record GlobalEngagementAssignmentManager(IGlobalEngagementAssignmentStore Store, GlobalEngagementAssignmentValidator Validator, IClock Clock, List<string> Journal, GlobalEngagementAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global engagement assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalEngagementAssignmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global engagement assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalEngagementAssignmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global engagement assignment {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of global engagement assignment workflow.</summary>
public static class GlobalEngagementAssignmentWorkflow
{
    public static GlobalEngagementAssignmentManager NewGlobalEngagementAssignmentManager(IGlobalEngagementAssignmentStore store, IClock clock)
    {
        return new GlobalEngagementAssignmentManager(
            store,
            GlobalEngagementAssignmentRules.DefaultGlobalEngagementAssignmentValidator(),
            clock,
            new List<string>(),
            GlobalEngagementAssignmentEngine.DefaultGlobalEngagementAssignmentEngine());
    }
}
