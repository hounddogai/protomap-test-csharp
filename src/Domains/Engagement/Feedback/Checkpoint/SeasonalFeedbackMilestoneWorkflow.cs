namespace Acme.Workforce.Engagement.Feedback.Checkpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal feedback milestones through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalFeedbackMilestoneDesk(ISeasonalFeedbackMilestoneStore Store, SeasonalFeedbackMilestoneValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal feedback milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalFeedbackMilestoneStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal feedback milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalFeedbackMilestoneStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of seasonal feedback milestone workflow.</summary>
public static class SeasonalFeedbackMilestoneWorkflow
{
    public static SeasonalFeedbackMilestoneDesk NewSeasonalFeedbackMilestoneDesk(ISeasonalFeedbackMilestoneStore store, IClock clock)
    {
        return new SeasonalFeedbackMilestoneDesk(
            store,
            SeasonalFeedbackMilestoneRules.DefaultSeasonalFeedbackMilestoneValidator(),
            clock,
            new List<string>());
    }
}
