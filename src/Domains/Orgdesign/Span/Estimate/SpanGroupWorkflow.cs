namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves span groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanGroupDesk(ISpanGroupStore Store, SpanGroupValidator Validator, IClock Clock, List<string> Journal, SpanGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanGroupStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanGroupStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span group {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of span group workflow.</summary>
public static class SpanGroupWorkflow
{
    public static SpanGroupDesk NewSpanGroupDesk(ISpanGroupStore store, IClock clock, Registry flags)
    {
        return new SpanGroupDesk(
            store,
            SpanGroupRules.DefaultSpanGroupValidator(),
            clock,
            new List<string>(),
            SpanGroupEngine.DefaultSpanGroupEngine(flags));
    }
}
