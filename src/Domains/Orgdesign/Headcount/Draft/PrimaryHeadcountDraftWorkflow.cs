namespace Acme.Workforce.Orgdesign.Headcount.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary headcount drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryHeadcountDraftCoordinator(IPrimaryHeadcountDraftStore Store, PrimaryHeadcountDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary headcount draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryHeadcountDraftStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary headcount draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryHeadcountDraftStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of primary headcount draft workflow.</summary>
public static class PrimaryHeadcountDraftWorkflow
{
    public static PrimaryHeadcountDraftCoordinator NewPrimaryHeadcountDraftCoordinator(IPrimaryHeadcountDraftStore store, IClock clock)
    {
        return new PrimaryHeadcountDraftCoordinator(
            store,
            PrimaryHeadcountDraftRules.DefaultPrimaryHeadcountDraftValidator(),
            clock,
            new List<string>());
    }
}
