namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled adjustment requests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledAdjustmentRequestCoordinator(IScheduledAdjustmentRequestStore Store, ScheduledAdjustmentRequestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled adjustment request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledAdjustmentRequestStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled adjustment request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledAdjustmentRequestStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of scheduled adjustment request workflow.</summary>
public static class ScheduledAdjustmentRequestWorkflow
{
    public static ScheduledAdjustmentRequestCoordinator NewScheduledAdjustmentRequestCoordinator(IScheduledAdjustmentRequestStore store, IClock clock)
    {
        return new ScheduledAdjustmentRequestCoordinator(
            store,
            ScheduledAdjustmentRequestRules.DefaultScheduledAdjustmentRequestValidator(),
            clock,
            new List<string>());
    }
}
