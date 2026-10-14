namespace Acme.Workforce.Compensation.PayBand.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled pay band reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledPayBandReviewCoordinator(IScheduledPayBandReviewStore Store, ScheduledPayBandReviewValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay band review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledPayBandReviewStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay band review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledPayBandReviewStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of scheduled pay band review workflow.</summary>
public static class ScheduledPayBandReviewWorkflow
{
    public static ScheduledPayBandReviewCoordinator NewScheduledPayBandReviewCoordinator(IScheduledPayBandReviewStore store, IClock clock)
    {
        return new ScheduledPayBandReviewCoordinator(
            store,
            ScheduledPayBandReviewRules.DefaultScheduledPayBandReviewValidator(),
            clock,
            new List<string>());
    }
}
