namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient bonus pool drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientBonusPoolDraftWorkflow(ILenientBonusPoolDraftStore Store, LenientBonusPoolDraftValidator Validator, IClock Clock, List<string> Journal)
{
    public static LenientBonusPoolDraftWorkflow NewLenientBonusPoolDraftWorkflow(ILenientBonusPoolDraftStore store, IClock clock)
    {
        return new LenientBonusPoolDraftWorkflow(
            store,
            LenientBonusPoolDraftRules.DefaultLenientBonusPoolDraftValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient bonus pool draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientBonusPoolDraftStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient bonus pool draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientBonusPoolDraftStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
