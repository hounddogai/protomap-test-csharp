namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves position drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionDraftManager(IPositionDraftStore Store, PositionDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionDraftStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionDraftStatus.Archived);
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

/// <summary>Functions of position draft workflow.</summary>
public static class PositionDraftWorkflow
{
    public static PositionDraftManager NewPositionDraftManager(IPositionDraftStore store, IClock clock)
    {
        return new PositionDraftManager(
            store,
            PositionDraftRules.DefaultPositionDraftValidator(),
            clock,
            new List<string>());
    }
}
