namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves equity grant scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record EquityGrantScoreDesk(IEquityGrantScoreStore Store, EquityGrantScoreValidator Validator, IClock Clock, List<string> Journal, EquityGrantScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EquityGrantScoreStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EquityGrantScoreStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant score {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of equity grant score workflow.</summary>
public static class EquityGrantScoreWorkflow
{
    public static EquityGrantScoreDesk NewEquityGrantScoreDesk(IEquityGrantScoreStore store, IClock clock)
    {
        return new EquityGrantScoreDesk(
            store,
            EquityGrantScoreRules.DefaultEquityGrantScoreValidator(),
            clock,
            new List<string>(),
            EquityGrantScoreEngine.DefaultEquityGrantScoreEngine());
    }
}
