namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves benchmark policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record BenchmarkPolicyDesk(IBenchmarkPolicyStore Store, BenchmarkPolicyValidator Validator, IClock Clock, List<string> Journal, BenchmarkPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BenchmarkPolicyStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BenchmarkPolicyStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark policy {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of benchmark policy workflow.</summary>
public static class BenchmarkPolicyWorkflow
{
    public static BenchmarkPolicyDesk NewBenchmarkPolicyDesk(IBenchmarkPolicyStore store, IClock clock, Registry flags)
    {
        return new BenchmarkPolicyDesk(
            store,
            BenchmarkPolicyRules.DefaultBenchmarkPolicyValidator(),
            clock,
            new List<string>(),
            BenchmarkPolicyEngine.DefaultBenchmarkPolicyEngine(flags));
    }
}
