namespace Acme.Workforce.Engagement.Sentiment.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves sentiment milestones through their life cycle and keeps a journal of what it did.</summary>
public sealed record SentimentMilestoneManager(ISentimentMilestoneStore Store, SentimentMilestoneValidator Validator, IClock Clock, List<string> Journal, SentimentMilestoneEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SentimentMilestoneStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SentimentMilestoneStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment milestone {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of sentiment milestone workflow.</summary>
public static class SentimentMilestoneWorkflow
{
    public static SentimentMilestoneManager NewSentimentMilestoneManager(ISentimentMilestoneStore store, IClock clock, Registry flags)
    {
        return new SentimentMilestoneManager(
            store,
            SentimentMilestoneRules.DefaultSentimentMilestoneValidator(),
            clock,
            new List<string>(),
            SentimentMilestoneEngine.DefaultSentimentMilestoneEngine(flags));
    }
}
