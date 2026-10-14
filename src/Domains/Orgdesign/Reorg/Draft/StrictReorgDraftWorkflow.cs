namespace Acme.Workforce.Orgdesign.Reorg.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict reorg drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictReorgDraftService(IStrictReorgDraftStore Store, StrictReorgDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict reorg draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictReorgDraftStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict reorg draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictReorgDraftStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of strict reorg draft workflow.</summary>
public static class StrictReorgDraftWorkflow
{
    public static StrictReorgDraftService NewStrictReorgDraftService(IStrictReorgDraftStore store, IClock clock)
    {
        return new StrictReorgDraftService(
            store,
            StrictReorgDraftRules.DefaultStrictReorgDraftValidator(),
            clock,
            new List<string>());
    }
}
