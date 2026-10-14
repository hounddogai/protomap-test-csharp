namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped scenario ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedScenarioLedgerManager(ICappedScenarioLedgerStore Store, CappedScenarioLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped scenario ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedScenarioLedgerStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped scenario ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedScenarioLedgerStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of capped scenario ledger workflow.</summary>
public static class CappedScenarioLedgerWorkflow
{
    public static CappedScenarioLedgerManager NewCappedScenarioLedgerManager(ICappedScenarioLedgerStore store, IClock clock)
    {
        return new CappedScenarioLedgerManager(
            store,
            CappedScenarioLedgerRules.DefaultCappedScenarioLedgerValidator(),
            clock,
            new List<string>());
    }
}
