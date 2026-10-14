namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal access badge drafts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalAccessBadgeDraftService(ISeasonalAccessBadgeDraftStore Store, SeasonalAccessBadgeDraftValidator Validator, IClock Clock, List<string> Journal, SeasonalAccessBadgeDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal access badge draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalAccessBadgeDraftStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal access badge draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalAccessBadgeDraftStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal access badge draft {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of seasonal access badge draft workflow.</summary>
public static class SeasonalAccessBadgeDraftWorkflow
{
    public static SeasonalAccessBadgeDraftService NewSeasonalAccessBadgeDraftService(ISeasonalAccessBadgeDraftStore store, IClock clock)
    {
        return new SeasonalAccessBadgeDraftService(
            store,
            SeasonalAccessBadgeDraftRules.DefaultSeasonalAccessBadgeDraftValidator(),
            clock,
            new List<string>(),
            SeasonalAccessBadgeDraftEngine.DefaultSeasonalAccessBadgeDraftEngine());
    }
}
