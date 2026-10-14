namespace Acme.Workforce.Talent.Pool.PoolAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback pool drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackPoolDraftService(IFallbackPoolDraftStore Store, FallbackPoolDraftValidator Validator, IClock Clock, List<string> Journal, FallbackPoolDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback pool draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackPoolDraftStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback pool draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackPoolDraftStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback pool draft {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of fallback pool draft workflow.</summary>
public static class FallbackPoolDraftWorkflow
{
    public static FallbackPoolDraftService NewFallbackPoolDraftService(IFallbackPoolDraftStore store, IClock clock, Registry flags)
    {
        return new FallbackPoolDraftService(
            store,
            FallbackPoolDraftRules.DefaultFallbackPoolDraftValidator(),
            clock,
            new List<string>(),
            FallbackPoolDraftEngine.DefaultFallbackPoolDraftEngine(flags));
    }
}
