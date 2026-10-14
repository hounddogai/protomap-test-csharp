namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped tax form scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedTaxFormScoreDesk(ICappedTaxFormScoreStore Store, CappedTaxFormScoreValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped tax form score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedTaxFormScoreStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped tax form score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedTaxFormScoreStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of capped tax form score workflow.</summary>
public static class CappedTaxFormScoreWorkflow
{
    public static CappedTaxFormScoreDesk NewCappedTaxFormScoreDesk(ICappedTaxFormScoreStore store, IClock clock)
    {
        return new CappedTaxFormScoreDesk(
            store,
            CappedTaxFormScoreRules.DefaultCappedTaxFormScoreValidator(),
            clock,
            new List<string>());
    }
}
