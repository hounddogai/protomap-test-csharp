namespace Acme.Workforce.Engagement.Feedback.FeedbackGroup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves feedback budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record FeedbackBudgetManager(IFeedbackBudgetStore Store, FeedbackBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FeedbackBudgetStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FeedbackBudgetStatus.Closed);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of feedback budget workflow.</summary>
public static class FeedbackBudgetWorkflow
{
    public static FeedbackBudgetManager NewFeedbackBudgetManager(IFeedbackBudgetStore store, IClock clock)
    {
        return new FeedbackBudgetManager(
            store,
            FeedbackBudgetRules.DefaultFeedbackBudgetValidator(),
            clock,
            new List<string>());
    }
}
