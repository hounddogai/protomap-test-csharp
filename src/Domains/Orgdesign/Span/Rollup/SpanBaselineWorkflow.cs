namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves span baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanBaselineManager(ISpanBaselineStore Store, SpanBaselineValidator Validator, IClock Clock, List<string> Journal, SpanBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanBaselineStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanBaselineStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span baseline {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of span baseline workflow.</summary>
public static class SpanBaselineWorkflow
{
    public static SpanBaselineManager NewSpanBaselineManager(ISpanBaselineStore store, IClock clock)
    {
        return new SpanBaselineManager(
            store,
            SpanBaselineRules.DefaultSpanBaselineValidator(),
            clock,
            new List<string>(),
            SpanBaselineEngine.DefaultSpanBaselineEngine());
    }
}
