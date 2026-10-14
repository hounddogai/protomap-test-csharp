namespace Acme.Workforce.Talent.Nomination.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched nomination forecasts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedNominationForecastManager(IBatchedNominationForecastStore Store, BatchedNominationForecastValidator Validator, IClock Clock, List<string> Journal, BatchedNominationForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched nomination forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedNominationForecastStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched nomination forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedNominationForecastStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched nomination forecast {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of batched nomination forecast workflow.</summary>
public static class BatchedNominationForecastWorkflow
{
    public static BatchedNominationForecastManager NewBatchedNominationForecastManager(IBatchedNominationForecastStore store, IClock clock, Registry flags)
    {
        return new BatchedNominationForecastManager(
            store,
            BatchedNominationForecastRules.DefaultBatchedNominationForecastValidator(),
            clock,
            new List<string>(),
            BatchedNominationForecastEngine.DefaultBatchedNominationForecastEngine(flags));
    }
}
