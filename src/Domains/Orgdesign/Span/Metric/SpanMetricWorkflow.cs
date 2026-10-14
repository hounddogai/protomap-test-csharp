namespace Acme.Workforce.Orgdesign.Span.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves span metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanMetricManager(ISpanMetricStore Store, SpanMetricValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanMetricStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanMetricStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of span metric workflow.</summary>
public static class SpanMetricWorkflow
{
    public static SpanMetricManager NewSpanMetricManager(ISpanMetricStore store, IClock clock)
    {
        return new SpanMetricManager(store, SpanMetricRules.DefaultSpanMetricValidator(), clock, new List<string>());
    }
}
