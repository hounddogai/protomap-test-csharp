namespace Acme.Workforce.Contractors.Engagement.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped engagement summaries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedEngagementSummaryCoordinator(ICappedEngagementSummaryStore Store, CappedEngagementSummaryValidator Validator, IClock Clock, List<string> Journal, CappedEngagementSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped engagement summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedEngagementSummaryStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped engagement summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedEngagementSummaryStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped engagement summary {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of capped engagement summary workflow.</summary>
public static class CappedEngagementSummaryWorkflow
{
    public static CappedEngagementSummaryCoordinator NewCappedEngagementSummaryCoordinator(ICappedEngagementSummaryStore store, IClock clock, Registry flags)
    {
        return new CappedEngagementSummaryCoordinator(
            store,
            CappedEngagementSummaryRules.DefaultCappedEngagementSummaryValidator(),
            clock,
            new List<string>(),
            CappedEngagementSummaryEngine.DefaultCappedEngagementSummaryEngine(flags));
    }
}
