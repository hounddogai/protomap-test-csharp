namespace Acme.Workforce.Compensation.EquityGrant.GrantRecord;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled equity grant variances through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledEquityGrantVarianceService(IScheduledEquityGrantVarianceStore Store, ScheduledEquityGrantVarianceValidator Validator, IClock Clock, List<string> Journal, ScheduledEquityGrantVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled equity grant variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledEquityGrantVarianceStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled equity grant variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledEquityGrantVarianceStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled equity grant variance {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of scheduled equity grant variance workflow.</summary>
public static class ScheduledEquityGrantVarianceWorkflow
{
    public static ScheduledEquityGrantVarianceService NewScheduledEquityGrantVarianceService(IScheduledEquityGrantVarianceStore store, IClock clock)
    {
        return new ScheduledEquityGrantVarianceService(
            store,
            ScheduledEquityGrantVarianceRules.DefaultScheduledEquityGrantVarianceValidator(),
            clock,
            new List<string>(),
            ScheduledEquityGrantVarianceEngine.DefaultScheduledEquityGrantVarianceEngine());
    }
}
