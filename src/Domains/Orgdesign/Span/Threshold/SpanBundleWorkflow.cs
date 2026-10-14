namespace Acme.Workforce.Orgdesign.Span.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves span bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanBundleCoordinator(ISpanBundleStore Store, SpanBundleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanBundleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanBundleStatus.Archived);
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

/// <summary>Functions of span bundle workflow.</summary>
public static class SpanBundleWorkflow
{
    public static SpanBundleCoordinator NewSpanBundleCoordinator(ISpanBundleStore store, IClock clock)
    {
        return new SpanBundleCoordinator(
            store,
            SpanBundleRules.DefaultSpanBundleValidator(),
            clock,
            new List<string>());
    }
}
