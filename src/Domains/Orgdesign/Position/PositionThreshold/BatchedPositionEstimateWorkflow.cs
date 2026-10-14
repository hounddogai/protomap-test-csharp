namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched position estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedPositionEstimateCoordinator(IBatchedPositionEstimateStore Store, BatchedPositionEstimateValidator Validator, IClock Clock, List<string> Journal, BatchedPositionEstimateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched position estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedPositionEstimateStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched position estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedPositionEstimateStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched position estimate {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of batched position estimate workflow.</summary>
public static class BatchedPositionEstimateWorkflow
{
    public static BatchedPositionEstimateCoordinator NewBatchedPositionEstimateCoordinator(IBatchedPositionEstimateStore store, IClock clock)
    {
        return new BatchedPositionEstimateCoordinator(
            store,
            BatchedPositionEstimateRules.DefaultBatchedPositionEstimateValidator(),
            clock,
            new List<string>(),
            BatchedPositionEstimateEngine.DefaultBatchedPositionEstimateEngine());
    }
}
