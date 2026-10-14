namespace Acme.Workforce.Orgdesign.Scenario.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scenario ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioLedgerManager(IScenarioLedgerStore Store, ScenarioLedgerValidator Validator, IClock Clock, List<string> Journal, ScenarioLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioLedgerStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioLedgerStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario ledger {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of scenario ledger workflow.</summary>
public static class ScenarioLedgerWorkflow
{
    public static ScenarioLedgerManager NewScenarioLedgerManager(IScenarioLedgerStore store, IClock clock)
    {
        return new ScenarioLedgerManager(
            store,
            ScenarioLedgerRules.DefaultScenarioLedgerValidator(),
            clock,
            new List<string>(),
            ScenarioLedgerEngine.DefaultScenarioLedgerEngine());
    }
}
