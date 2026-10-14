namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves span snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanSnapshotCoordinator(ISpanSnapshotStore Store, SpanSnapshotValidator Validator, IClock Clock, List<string> Journal, SpanSnapshotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanSnapshotStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanSnapshotStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span snapshot {code} not found");
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

/// <summary>Functions of span snapshot workflow.</summary>
public static class SpanSnapshotWorkflow
{
    public static SpanSnapshotCoordinator NewSpanSnapshotCoordinator(ISpanSnapshotStore store, IClock clock)
    {
        return new SpanSnapshotCoordinator(
            store,
            SpanSnapshotRules.DefaultSpanSnapshotValidator(),
            clock,
            new List<string>(),
            SpanSnapshotEngine.DefaultSpanSnapshotEngine());
    }
}
