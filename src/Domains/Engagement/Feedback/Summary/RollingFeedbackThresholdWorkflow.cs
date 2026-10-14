namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling feedback thresholds through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingFeedbackThresholdManager(IRollingFeedbackThresholdStore Store, RollingFeedbackThresholdValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling feedback threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingFeedbackThresholdStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling feedback threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingFeedbackThresholdStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of rolling feedback threshold workflow.</summary>
public static class RollingFeedbackThresholdWorkflow
{
    public static RollingFeedbackThresholdManager NewRollingFeedbackThresholdManager(IRollingFeedbackThresholdStore store, IClock clock)
    {
        return new RollingFeedbackThresholdManager(
            store,
            RollingFeedbackThresholdRules.DefaultRollingFeedbackThresholdValidator(),
            clock,
            new List<string>());
    }
}
