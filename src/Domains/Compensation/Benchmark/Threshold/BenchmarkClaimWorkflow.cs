namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves benchmark claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record BenchmarkClaimService(IBenchmarkClaimStore Store, BenchmarkClaimValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BenchmarkClaimStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BenchmarkClaimStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of benchmark claim workflow.</summary>
public static class BenchmarkClaimWorkflow
{
    public static BenchmarkClaimService NewBenchmarkClaimService(IBenchmarkClaimStore store, IClock clock)
    {
        return new BenchmarkClaimService(
            store,
            BenchmarkClaimRules.DefaultBenchmarkClaimValidator(),
            clock,
            new List<string>());
    }
}
