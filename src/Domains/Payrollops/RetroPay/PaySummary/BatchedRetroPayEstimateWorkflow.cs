namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched retro pay estimates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedRetroPayEstimateCoordinator(IBatchedRetroPayEstimateStore Store, BatchedRetroPayEstimateValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched retro pay estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedRetroPayEstimateStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched retro pay estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedRetroPayEstimateStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of batched retro pay estimate workflow.</summary>
public static class BatchedRetroPayEstimateWorkflow
{
    public static BatchedRetroPayEstimateCoordinator NewBatchedRetroPayEstimateCoordinator(IBatchedRetroPayEstimateStore store, IClock clock)
    {
        return new BatchedRetroPayEstimateCoordinator(
            store,
            BatchedRetroPayEstimateRules.DefaultBatchedRetroPayEstimateValidator(),
            clock,
            new List<string>());
    }
}
