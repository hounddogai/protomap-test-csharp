namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled adjustment summaries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledAdjustmentSummaryManager(IScheduledAdjustmentSummaryStore Store, ScheduledAdjustmentSummaryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled adjustment summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledAdjustmentSummaryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled adjustment summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledAdjustmentSummaryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of scheduled adjustment summary workflow.</summary>
public static class ScheduledAdjustmentSummaryWorkflow
{
    public static ScheduledAdjustmentSummaryManager NewScheduledAdjustmentSummaryManager(IScheduledAdjustmentSummaryStore store, IClock clock)
    {
        return new ScheduledAdjustmentSummaryManager(
            store,
            ScheduledAdjustmentSummaryRules.DefaultScheduledAdjustmentSummaryValidator(),
            clock,
            new List<string>());
    }
}
