namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal nomination ledgers through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalNominationLedgerCoordinator(ISeasonalNominationLedgerStore Store, SeasonalNominationLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal nomination ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalNominationLedgerStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal nomination ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalNominationLedgerStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of seasonal nomination ledger workflow.</summary>
public static class SeasonalNominationLedgerWorkflow
{
    public static SeasonalNominationLedgerCoordinator NewSeasonalNominationLedgerCoordinator(ISeasonalNominationLedgerStore store, IClock clock)
    {
        return new SeasonalNominationLedgerCoordinator(
            store,
            SeasonalNominationLedgerRules.DefaultSeasonalNominationLedgerValidator(),
            clock,
            new List<string>());
    }
}
