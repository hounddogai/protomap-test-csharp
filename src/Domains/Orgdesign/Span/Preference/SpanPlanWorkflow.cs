namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves span plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanPlanWorkflow(ISpanPlanStore Store, SpanPlanValidator Validator, IClock Clock, List<string> Journal, SpanPlanEngine Engine)
{
    public static SpanPlanWorkflow NewSpanPlanWorkflow(ISpanPlanStore store, IClock clock)
    {
        return new SpanPlanWorkflow(
            store,
            SpanPlanRules.DefaultSpanPlanValidator(),
            clock,
            new List<string>(),
            SpanPlanEngine.DefaultSpanPlanEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanPlanStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanPlanStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span plan {code} not found");
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
