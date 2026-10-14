namespace Acme.Workforce.Absence.Accrual.AccrualAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled accrual assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledAccrualAssignmentDesk(IScheduledAccrualAssignmentStore Store, ScheduledAccrualAssignmentValidator Validator, IClock Clock, List<string> Journal, ScheduledAccrualAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled accrual assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledAccrualAssignmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled accrual assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledAccrualAssignmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled accrual assignment {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of scheduled accrual assignment workflow.</summary>
public static class ScheduledAccrualAssignmentWorkflow
{
    public static ScheduledAccrualAssignmentDesk NewScheduledAccrualAssignmentDesk(IScheduledAccrualAssignmentStore store, IClock clock, Registry flags)
    {
        return new ScheduledAccrualAssignmentDesk(
            store,
            ScheduledAccrualAssignmentRules.DefaultScheduledAccrualAssignmentValidator(),
            clock,
            new List<string>(),
            ScheduledAccrualAssignmentEngine.DefaultScheduledAccrualAssignmentEngine(flags));
    }
}
