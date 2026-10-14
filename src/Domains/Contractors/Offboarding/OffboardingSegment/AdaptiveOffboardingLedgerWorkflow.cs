namespace Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive offboarding ledgers through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveOffboardingLedgerCoordinator(IAdaptiveOffboardingLedgerStore Store, AdaptiveOffboardingLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive offboarding ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveOffboardingLedgerStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive offboarding ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveOffboardingLedgerStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of adaptive offboarding ledger workflow.</summary>
public static class AdaptiveOffboardingLedgerWorkflow
{
    public static AdaptiveOffboardingLedgerCoordinator NewAdaptiveOffboardingLedgerCoordinator(IAdaptiveOffboardingLedgerStore store, IClock clock)
    {
        return new AdaptiveOffboardingLedgerCoordinator(
            store,
            AdaptiveOffboardingLedgerRules.DefaultAdaptiveOffboardingLedgerValidator(),
            clock,
            new List<string>());
    }
}
