namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves span summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanSummaryCoordinator(ISpanSummaryStore Store, SpanSummaryValidator Validator, IClock Clock, List<string> Journal, SpanSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanSummaryStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanSummaryStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span summary {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of span summary workflow.</summary>
public static class SpanSummaryWorkflow
{
    public static SpanSummaryCoordinator NewSpanSummaryCoordinator(ISpanSummaryStore store, IClock clock)
    {
        return new SpanSummaryCoordinator(
            store,
            SpanSummaryRules.DefaultSpanSummaryValidator(),
            clock,
            new List<string>(),
            SpanSummaryEngine.DefaultSpanSummaryEngine());
    }
}
