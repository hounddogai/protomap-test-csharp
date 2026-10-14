namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves nomination ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record NominationLedgerCoordinator(INominationLedgerStore Store, NominationLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(NominationLedgerStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(NominationLedgerStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of nomination ledger workflow.</summary>
public static class NominationLedgerWorkflow
{
    public static NominationLedgerCoordinator NewNominationLedgerCoordinator(INominationLedgerStore store, IClock clock)
    {
        return new NominationLedgerCoordinator(
            store,
            NominationLedgerRules.DefaultNominationLedgerValidator(),
            clock,
            new List<string>());
    }
}
