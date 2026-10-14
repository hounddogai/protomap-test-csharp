namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched laptop variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedLaptopVarianceDesk(IBatchedLaptopVarianceStore Store, BatchedLaptopVarianceValidator Validator, IClock Clock, List<string> Journal, BatchedLaptopVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched laptop variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedLaptopVarianceStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched laptop variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedLaptopVarianceStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched laptop variance {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of batched laptop variance workflow.</summary>
public static class BatchedLaptopVarianceWorkflow
{
    public static BatchedLaptopVarianceDesk NewBatchedLaptopVarianceDesk(IBatchedLaptopVarianceStore store, IClock clock, Registry flags)
    {
        return new BatchedLaptopVarianceDesk(
            store,
            BatchedLaptopVarianceRules.DefaultBatchedLaptopVarianceValidator(),
            clock,
            new List<string>(),
            BatchedLaptopVarianceEngine.DefaultBatchedLaptopVarianceEngine(flags));
    }
}
