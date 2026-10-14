namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive leave request ledgers through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveLeaveRequestLedgerCoordinator(IAdaptiveLeaveRequestLedgerStore Store, AdaptiveLeaveRequestLedgerValidator Validator, IClock Clock, List<string> Journal, AdaptiveLeaveRequestLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive leave request ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveLeaveRequestLedgerStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive leave request ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveLeaveRequestLedgerStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive leave request ledger {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of adaptive leave request ledger workflow.</summary>
public static class AdaptiveLeaveRequestLedgerWorkflow
{
    public static AdaptiveLeaveRequestLedgerCoordinator NewAdaptiveLeaveRequestLedgerCoordinator(IAdaptiveLeaveRequestLedgerStore store, IClock clock)
    {
        return new AdaptiveLeaveRequestLedgerCoordinator(
            store,
            AdaptiveLeaveRequestLedgerRules.DefaultAdaptiveLeaveRequestLedgerValidator(),
            clock,
            new List<string>(),
            AdaptiveLeaveRequestLedgerEngine.DefaultAdaptiveLeaveRequestLedgerEngine());
    }
}
