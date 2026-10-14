namespace Acme.Workforce.Engagement.Feedback.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves feedback schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record FeedbackScheduleWorkflow(IFeedbackScheduleStore Store, FeedbackScheduleValidator Validator, IClock Clock, List<string> Journal)
{
    public static FeedbackScheduleWorkflow NewFeedbackScheduleWorkflow(IFeedbackScheduleStore store, IClock clock)
    {
        return new FeedbackScheduleWorkflow(
            store,
            FeedbackScheduleRules.DefaultFeedbackScheduleValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FeedbackScheduleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FeedbackScheduleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
