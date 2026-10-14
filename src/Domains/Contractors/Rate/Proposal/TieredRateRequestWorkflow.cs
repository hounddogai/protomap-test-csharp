namespace Acme.Workforce.Contractors.Rate.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered rate requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredRateRequestService(ITieredRateRequestStore Store, TieredRateRequestValidator Validator, IClock Clock, List<string> Journal, TieredRateRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered rate request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredRateRequestStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered rate request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredRateRequestStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered rate request {code} not found");
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

/// <summary>Functions of tiered rate request workflow.</summary>
public static class TieredRateRequestWorkflow
{
    public static TieredRateRequestService NewTieredRateRequestService(ITieredRateRequestStore store, IClock clock, Registry flags)
    {
        return new TieredRateRequestService(
            store,
            TieredRateRequestRules.DefaultTieredRateRequestValidator(),
            clock,
            new List<string>(),
            TieredRateRequestEngine.DefaultTieredRateRequestEngine(flags));
    }
}
