namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched pay band thresholds through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedPayBandThresholdManager(IBatchedPayBandThresholdStore Store, BatchedPayBandThresholdValidator Validator, IClock Clock, List<string> Journal, BatchedPayBandThresholdEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched pay band threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedPayBandThresholdStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched pay band threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedPayBandThresholdStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched pay band threshold {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of batched pay band threshold workflow.</summary>
public static class BatchedPayBandThresholdWorkflow
{
    public static BatchedPayBandThresholdManager NewBatchedPayBandThresholdManager(IBatchedPayBandThresholdStore store, IClock clock, Registry flags)
    {
        return new BatchedPayBandThresholdManager(
            store,
            BatchedPayBandThresholdRules.DefaultBatchedPayBandThresholdValidator(),
            clock,
            new List<string>(),
            BatchedPayBandThresholdEngine.DefaultBatchedPayBandThresholdEngine(flags));
    }
}
