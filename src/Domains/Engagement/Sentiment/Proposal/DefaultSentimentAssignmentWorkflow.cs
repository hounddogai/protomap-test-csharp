namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default sentiment assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultSentimentAssignmentService(IDefaultSentimentAssignmentStore Store, DefaultSentimentAssignmentValidator Validator, IClock Clock, List<string> Journal, DefaultSentimentAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default sentiment assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultSentimentAssignmentStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default sentiment assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultSentimentAssignmentStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default sentiment assignment {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of default sentiment assignment workflow.</summary>
public static class DefaultSentimentAssignmentWorkflow
{
    public static DefaultSentimentAssignmentService NewDefaultSentimentAssignmentService(IDefaultSentimentAssignmentStore store, IClock clock)
    {
        return new DefaultSentimentAssignmentService(
            store,
            DefaultSentimentAssignmentRules.DefaultDefaultSentimentAssignmentValidator(),
            clock,
            new List<string>(),
            DefaultSentimentAssignmentEngine.DefaultDefaultSentimentAssignmentEngine());
    }
}
