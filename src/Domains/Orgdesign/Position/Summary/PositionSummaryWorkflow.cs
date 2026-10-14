namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves position summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionSummaryService(IPositionSummaryStore Store, PositionSummaryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionSummaryStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionSummaryStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of position summary workflow.</summary>
public static class PositionSummaryWorkflow
{
    public static PositionSummaryService NewPositionSummaryService(IPositionSummaryStore store, IClock clock)
    {
        return new PositionSummaryService(
            store,
            PositionSummaryRules.DefaultPositionSummaryValidator(),
            clock,
            new List<string>());
    }
}
