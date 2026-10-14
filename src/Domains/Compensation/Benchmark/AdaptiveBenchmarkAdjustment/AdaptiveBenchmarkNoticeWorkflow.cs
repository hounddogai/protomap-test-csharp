namespace Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive benchmark notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveBenchmarkNoticeCoordinator(IAdaptiveBenchmarkNoticeStore Store, AdaptiveBenchmarkNoticeValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive benchmark notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveBenchmarkNoticeStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive benchmark notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveBenchmarkNoticeStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of adaptive benchmark notice workflow.</summary>
public static class AdaptiveBenchmarkNoticeWorkflow
{
    public static AdaptiveBenchmarkNoticeCoordinator NewAdaptiveBenchmarkNoticeCoordinator(IAdaptiveBenchmarkNoticeStore store, IClock clock)
    {
        return new AdaptiveBenchmarkNoticeCoordinator(
            store,
            AdaptiveBenchmarkNoticeRules.DefaultAdaptiveBenchmarkNoticeValidator(),
            clock,
            new List<string>());
    }
}
