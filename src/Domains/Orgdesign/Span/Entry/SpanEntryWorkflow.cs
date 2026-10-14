namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves span entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanEntryDesk(ISpanEntryStore Store, SpanEntryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanEntryStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanEntryStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of span entry workflow.</summary>
public static class SpanEntryWorkflow
{
    public static SpanEntryDesk NewSpanEntryDesk(ISpanEntryStore store, IClock clock)
    {
        return new SpanEntryDesk(store, SpanEntryRules.DefaultSpanEntryValidator(), clock, new List<string>());
    }
}
