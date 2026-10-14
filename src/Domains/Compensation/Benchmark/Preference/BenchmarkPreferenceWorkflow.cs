namespace Acme.Workforce.Compensation.Benchmark.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves benchmark preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record BenchmarkPreferenceWorkflow(IBenchmarkPreferenceStore Store, BenchmarkPreferenceValidator Validator, IClock Clock, List<string> Journal, BenchmarkPreferenceEngine Engine)
{
    public static BenchmarkPreferenceWorkflow NewBenchmarkPreferenceWorkflow(IBenchmarkPreferenceStore store, IClock clock)
    {
        return new BenchmarkPreferenceWorkflow(
            store,
            BenchmarkPreferenceRules.DefaultBenchmarkPreferenceValidator(),
            clock,
            new List<string>(),
            BenchmarkPreferenceEngine.DefaultBenchmarkPreferenceEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BenchmarkPreferenceStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BenchmarkPreferenceStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark preference {code} not found");
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
            if (item.StartsAt > cutoff)
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
