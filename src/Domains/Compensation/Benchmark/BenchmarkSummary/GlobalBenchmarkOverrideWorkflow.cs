namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global benchmark overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalBenchmarkOverrideWorkflow(IGlobalBenchmarkOverrideStore Store, GlobalBenchmarkOverrideValidator Validator, IClock Clock, List<string> Journal)
{
    public static GlobalBenchmarkOverrideWorkflow NewGlobalBenchmarkOverrideWorkflow(IGlobalBenchmarkOverrideStore store, IClock clock)
    {
        return new GlobalBenchmarkOverrideWorkflow(
            store,
            GlobalBenchmarkOverrideRules.DefaultGlobalBenchmarkOverrideValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global benchmark override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalBenchmarkOverrideStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global benchmark override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalBenchmarkOverrideStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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
