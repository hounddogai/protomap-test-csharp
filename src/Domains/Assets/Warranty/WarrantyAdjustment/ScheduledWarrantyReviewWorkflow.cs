namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled warranty reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledWarrantyReviewManager(IScheduledWarrantyReviewStore Store, ScheduledWarrantyReviewValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled warranty review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledWarrantyReviewStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled warranty review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledWarrantyReviewStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of scheduled warranty review workflow.</summary>
public static class ScheduledWarrantyReviewWorkflow
{
    public static ScheduledWarrantyReviewManager NewScheduledWarrantyReviewManager(IScheduledWarrantyReviewStore store, IClock clock)
    {
        return new ScheduledWarrantyReviewManager(
            store,
            ScheduledWarrantyReviewRules.DefaultScheduledWarrantyReviewValidator(),
            clock,
            new List<string>());
    }
}
