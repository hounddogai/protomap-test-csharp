namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict leave request claims through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictLeaveRequestClaimDesk(IStrictLeaveRequestClaimStore Store, StrictLeaveRequestClaimValidator Validator, IClock Clock, List<string> Journal, StrictLeaveRequestClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict leave request claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictLeaveRequestClaimStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict leave request claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictLeaveRequestClaimStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict leave request claim {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of strict leave request claim workflow.</summary>
public static class StrictLeaveRequestClaimWorkflow
{
    public static StrictLeaveRequestClaimDesk NewStrictLeaveRequestClaimDesk(IStrictLeaveRequestClaimStore store, IClock clock, Registry flags)
    {
        return new StrictLeaveRequestClaimDesk(
            store,
            StrictLeaveRequestClaimRules.DefaultStrictLeaveRequestClaimValidator(),
            clock,
            new List<string>(),
            StrictLeaveRequestClaimEngine.DefaultStrictLeaveRequestClaimEngine(flags));
    }
}
