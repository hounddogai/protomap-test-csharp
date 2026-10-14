namespace Acme.Workforce.Contractors.Engagement.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves engagement scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record EngagementScoreCoordinator(IEngagementScoreStore Store, EngagementScoreValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EngagementScoreStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EngagementScoreStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of engagement score workflow.</summary>
public static class EngagementScoreWorkflow
{
    public static EngagementScoreCoordinator NewEngagementScoreCoordinator(IEngagementScoreStore store, IClock clock)
    {
        return new EngagementScoreCoordinator(
            store,
            EngagementScoreRules.DefaultEngagementScoreValidator(),
            clock,
            new List<string>());
    }
}
