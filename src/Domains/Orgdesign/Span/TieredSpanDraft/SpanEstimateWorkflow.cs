namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves span estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanEstimateCoordinator(ISpanEstimateStore Store, SpanEstimateValidator Validator, IClock Clock, List<string> Journal, SpanEstimateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanEstimateStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanEstimateStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span estimate {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of span estimate workflow.</summary>
public static class SpanEstimateWorkflow
{
    public static SpanEstimateCoordinator NewSpanEstimateCoordinator(ISpanEstimateStore store, IClock clock)
    {
        return new SpanEstimateCoordinator(
            store,
            SpanEstimateRules.DefaultSpanEstimateValidator(),
            clock,
            new List<string>(),
            SpanEstimateEngine.DefaultSpanEstimateEngine());
    }
}
