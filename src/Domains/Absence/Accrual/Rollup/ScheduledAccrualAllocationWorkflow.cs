namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled accrual allocations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledAccrualAllocationWorkflow(IScheduledAccrualAllocationStore Store, ScheduledAccrualAllocationValidator Validator, IClock Clock, List<string> Journal)
{
    public static ScheduledAccrualAllocationWorkflow NewScheduledAccrualAllocationWorkflow(IScheduledAccrualAllocationStore store, IClock clock)
    {
        return new ScheduledAccrualAllocationWorkflow(
            store,
            ScheduledAccrualAllocationRules.DefaultScheduledAccrualAllocationValidator(),
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
            throw new InvalidOperationException($"scheduled accrual allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledAccrualAllocationStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled accrual allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledAccrualAllocationStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public int Scan(int cutoff)
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
