namespace Acme.Workforce.Compensation.Adjustment.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback adjustment drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackAdjustmentDraftManager(IFallbackAdjustmentDraftStore Store, FallbackAdjustmentDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback adjustment draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackAdjustmentDraftStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback adjustment draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackAdjustmentDraftStatus.Archived);
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

/// <summary>Functions of fallback adjustment draft workflow.</summary>
public static class FallbackAdjustmentDraftWorkflow
{
    public static FallbackAdjustmentDraftManager NewFallbackAdjustmentDraftManager(IFallbackAdjustmentDraftStore store, IClock clock)
    {
        return new FallbackAdjustmentDraftManager(
            store,
            FallbackAdjustmentDraftRules.DefaultFallbackAdjustmentDraftValidator(),
            clock,
            new List<string>());
    }
}
