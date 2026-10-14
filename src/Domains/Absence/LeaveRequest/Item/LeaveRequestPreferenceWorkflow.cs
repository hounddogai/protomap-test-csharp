namespace Acme.Workforce.Absence.LeaveRequest.Item;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves leave request preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestPreferenceCoordinator(ILeaveRequestPreferenceStore Store, LeaveRequestPreferenceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestPreferenceStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestPreferenceStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of leave request preference workflow.</summary>
public static class LeaveRequestPreferenceWorkflow
{
    public static LeaveRequestPreferenceCoordinator NewLeaveRequestPreferenceCoordinator(ILeaveRequestPreferenceStore store, IClock clock)
    {
        return new LeaveRequestPreferenceCoordinator(
            store,
            LeaveRequestPreferenceRules.DefaultLeaveRequestPreferenceValidator(),
            clock,
            new List<string>());
    }
}
