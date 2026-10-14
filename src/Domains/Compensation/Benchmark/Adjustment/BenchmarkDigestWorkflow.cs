namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves benchmark digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record BenchmarkDigestDesk(IBenchmarkDigestStore Store, BenchmarkDigestValidator Validator, IClock Clock, List<string> Journal, BenchmarkDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BenchmarkDigestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BenchmarkDigestStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark digest {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of benchmark digest workflow.</summary>
public static class BenchmarkDigestWorkflow
{
    public static BenchmarkDigestDesk NewBenchmarkDigestDesk(IBenchmarkDigestStore store, IClock clock, Registry flags)
    {
        return new BenchmarkDigestDesk(
            store,
            BenchmarkDigestRules.DefaultBenchmarkDigestValidator(),
            clock,
            new List<string>(),
            BenchmarkDigestEngine.DefaultBenchmarkDigestEngine(flags));
    }
}
