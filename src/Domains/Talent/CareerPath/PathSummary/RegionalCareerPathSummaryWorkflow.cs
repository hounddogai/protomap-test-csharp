namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional career path summaries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalCareerPathSummaryService(IRegionalCareerPathSummaryStore Store, RegionalCareerPathSummaryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional career path summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalCareerPathSummaryStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional career path summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalCareerPathSummaryStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of regional career path summary workflow.</summary>
public static class RegionalCareerPathSummaryWorkflow
{
    public static RegionalCareerPathSummaryService NewRegionalCareerPathSummaryService(IRegionalCareerPathSummaryStore store, IClock clock)
    {
        return new RegionalCareerPathSummaryService(
            store,
            RegionalCareerPathSummaryRules.DefaultRegionalCareerPathSummaryValidator(),
            clock,
            new List<string>());
    }
}
