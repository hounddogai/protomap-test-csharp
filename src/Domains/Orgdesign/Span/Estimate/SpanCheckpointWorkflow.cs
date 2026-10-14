namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves span checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanCheckpointCoordinator(ISpanCheckpointStore Store, SpanCheckpointValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanCheckpointStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanCheckpointStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of span checkpoint workflow.</summary>
public static class SpanCheckpointWorkflow
{
    public static SpanCheckpointCoordinator NewSpanCheckpointCoordinator(ISpanCheckpointStore store, IClock clock)
    {
        return new SpanCheckpointCoordinator(
            store,
            SpanCheckpointRules.DefaultSpanCheckpointValidator(),
            clock,
            new List<string>());
    }
}
