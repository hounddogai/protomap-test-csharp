namespace Acme.Workforce.Engagement.Feedback.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves feedback rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record FeedbackRuleDesk(IFeedbackRuleStore Store, FeedbackRuleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FeedbackRuleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FeedbackRuleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of feedback rule workflow.</summary>
public static class FeedbackRuleWorkflow
{
    public static FeedbackRuleDesk NewFeedbackRuleDesk(IFeedbackRuleStore store, IClock clock)
    {
        return new FeedbackRuleDesk(
            store,
            FeedbackRuleRules.DefaultFeedbackRuleValidator(),
            clock,
            new List<string>());
    }
}
