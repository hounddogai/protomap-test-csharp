namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves span adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanAdjustmentWorkflow(ISpanAdjustmentStore Store, SpanAdjustmentValidator Validator, IClock Clock, List<string> Journal, SpanAdjustmentEngine Engine)
{
    public static SpanAdjustmentWorkflow NewSpanAdjustmentWorkflow(ISpanAdjustmentStore store, IClock clock)
    {
        return new SpanAdjustmentWorkflow(
            store,
            SpanAdjustmentRules.DefaultSpanAdjustmentValidator(),
            clock,
            new List<string>(),
            SpanAdjustmentEngine.DefaultSpanAdjustmentEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanAdjustmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanAdjustmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span adjustment {code} not found");
        }
        var result = Engine.Evaluate(item);
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
