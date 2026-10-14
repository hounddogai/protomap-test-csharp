namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves benchmark budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record BenchmarkBudgetManager(IBenchmarkBudgetStore Store, BenchmarkBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BenchmarkBudgetStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BenchmarkBudgetStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of benchmark budget workflow.</summary>
public static class BenchmarkBudgetWorkflow
{
    public static BenchmarkBudgetManager NewBenchmarkBudgetManager(IBenchmarkBudgetStore store, IClock clock)
    {
        return new BenchmarkBudgetManager(
            store,
            BenchmarkBudgetRules.DefaultBenchmarkBudgetValidator(),
            clock,
            new List<string>());
    }
}
