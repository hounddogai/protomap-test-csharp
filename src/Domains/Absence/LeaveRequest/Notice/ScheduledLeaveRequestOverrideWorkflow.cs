namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled leave request overrides through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledLeaveRequestOverrideService(IScheduledLeaveRequestOverrideStore Store, ScheduledLeaveRequestOverrideValidator Validator, IClock Clock, List<string> Journal, ScheduledLeaveRequestOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled leave request override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledLeaveRequestOverrideStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled leave request override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledLeaveRequestOverrideStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled leave request override {code} not found");
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

/// <summary>Functions of scheduled leave request override workflow.</summary>
public static class ScheduledLeaveRequestOverrideWorkflow
{
    public static ScheduledLeaveRequestOverrideService NewScheduledLeaveRequestOverrideService(IScheduledLeaveRequestOverrideStore store, IClock clock, Registry flags)
    {
        return new ScheduledLeaveRequestOverrideService(
            store,
            ScheduledLeaveRequestOverrideRules.DefaultScheduledLeaveRequestOverrideValidator(),
            clock,
            new List<string>(),
            ScheduledLeaveRequestOverrideEngine.DefaultScheduledLeaveRequestOverrideEngine(flags));
    }
}
