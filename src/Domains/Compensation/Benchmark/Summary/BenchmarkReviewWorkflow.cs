namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves benchmark reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record BenchmarkReviewService(IBenchmarkReviewStore Store, BenchmarkReviewValidator Validator, IClock Clock, List<string> Journal, BenchmarkReviewEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BenchmarkReviewStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BenchmarkReviewStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark review {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of benchmark review workflow.</summary>
public static class BenchmarkReviewWorkflow
{
    public static BenchmarkReviewService NewBenchmarkReviewService(IBenchmarkReviewStore store, IClock clock, Registry flags)
    {
        return new BenchmarkReviewService(
            store,
            BenchmarkReviewRules.DefaultBenchmarkReviewValidator(),
            clock,
            new List<string>(),
            BenchmarkReviewEngine.DefaultBenchmarkReviewEngine(flags));
    }
}
