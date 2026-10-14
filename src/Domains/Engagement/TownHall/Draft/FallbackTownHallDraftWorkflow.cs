namespace Acme.Workforce.Engagement.TownHall.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback town hall drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackTownHallDraftDesk(IFallbackTownHallDraftStore Store, FallbackTownHallDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback town hall draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackTownHallDraftStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback town hall draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackTownHallDraftStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of fallback town hall draft workflow.</summary>
public static class FallbackTownHallDraftWorkflow
{
    public static FallbackTownHallDraftDesk NewFallbackTownHallDraftDesk(IFallbackTownHallDraftStore store, IClock clock)
    {
        return new FallbackTownHallDraftDesk(
            store,
            FallbackTownHallDraftRules.DefaultFallbackTownHallDraftValidator(),
            clock,
            new List<string>());
    }
}
