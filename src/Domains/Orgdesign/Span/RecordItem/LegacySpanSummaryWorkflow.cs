namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy span summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacySpanSummaryCoordinator(ILegacySpanSummaryStore Store, LegacySpanSummaryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy span summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacySpanSummaryStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy span summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacySpanSummaryStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of legacy span summary workflow.</summary>
public static class LegacySpanSummaryWorkflow
{
    public static LegacySpanSummaryCoordinator NewLegacySpanSummaryCoordinator(ILegacySpanSummaryStore store, IClock clock)
    {
        return new LegacySpanSummaryCoordinator(
            store,
            LegacySpanSummaryRules.DefaultLegacySpanSummaryValidator(),
            clock,
            new List<string>());
    }
}
