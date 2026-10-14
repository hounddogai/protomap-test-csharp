namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped pay band thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedPayBandThresholdDesk(ICappedPayBandThresholdStore Store, CappedPayBandThresholdValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped pay band threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedPayBandThresholdStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped pay band threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedPayBandThresholdStatus.Stopped);
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

/// <summary>Functions of capped pay band threshold workflow.</summary>
public static class CappedPayBandThresholdWorkflow
{
    public static CappedPayBandThresholdDesk NewCappedPayBandThresholdDesk(ICappedPayBandThresholdStore store, IClock clock)
    {
        return new CappedPayBandThresholdDesk(
            store,
            CappedPayBandThresholdRules.DefaultCappedPayBandThresholdValidator(),
            clock,
            new List<string>());
    }
}
