namespace Acme.Workforce.Talent.Pool.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient pool drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientPoolDraftManager(ILenientPoolDraftStore Store, LenientPoolDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient pool draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientPoolDraftStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient pool draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientPoolDraftStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of lenient pool draft workflow.</summary>
public static class LenientPoolDraftWorkflow
{
    public static LenientPoolDraftManager NewLenientPoolDraftManager(ILenientPoolDraftStore store, IClock clock)
    {
        return new LenientPoolDraftManager(
            store,
            LenientPoolDraftRules.DefaultLenientPoolDraftValidator(),
            clock,
            new List<string>());
    }
}
