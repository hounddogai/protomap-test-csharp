namespace Acme.Workforce.Talent.CareerPath.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default career path summaries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultCareerPathSummaryDesk(IDefaultCareerPathSummaryStore Store, DefaultCareerPathSummaryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default career path summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultCareerPathSummaryStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default career path summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultCareerPathSummaryStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of default career path summary workflow.</summary>
public static class DefaultCareerPathSummaryWorkflow
{
    public static DefaultCareerPathSummaryDesk NewDefaultCareerPathSummaryDesk(IDefaultCareerPathSummaryStore store, IClock clock)
    {
        return new DefaultCareerPathSummaryDesk(
            store,
            DefaultCareerPathSummaryRules.DefaultDefaultCareerPathSummaryValidator(),
            clock,
            new List<string>());
    }
}
