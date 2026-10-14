namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered leave request overrides through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredLeaveRequestOverrideDesk(ITieredLeaveRequestOverrideStore Store, TieredLeaveRequestOverrideValidator Validator, IClock Clock, List<string> Journal, TieredLeaveRequestOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered leave request override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredLeaveRequestOverrideStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered leave request override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredLeaveRequestOverrideStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered leave request override {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of tiered leave request override workflow.</summary>
public static class TieredLeaveRequestOverrideWorkflow
{
    public static TieredLeaveRequestOverrideDesk NewTieredLeaveRequestOverrideDesk(ITieredLeaveRequestOverrideStore store, IClock clock, Registry flags)
    {
        return new TieredLeaveRequestOverrideDesk(
            store,
            TieredLeaveRequestOverrideRules.DefaultTieredLeaveRequestOverrideValidator(),
            clock,
            new List<string>(),
            TieredLeaveRequestOverrideEngine.DefaultTieredLeaveRequestOverrideEngine(flags));
    }
}
