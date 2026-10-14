namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback nomination bundles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackNominationBundleService(IFallbackNominationBundleStore Store, FallbackNominationBundleValidator Validator, IClock Clock, List<string> Journal, FallbackNominationBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback nomination bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackNominationBundleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback nomination bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackNominationBundleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback nomination bundle {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of fallback nomination bundle workflow.</summary>
public static class FallbackNominationBundleWorkflow
{
    public static FallbackNominationBundleService NewFallbackNominationBundleService(IFallbackNominationBundleStore store, IClock clock, Registry flags)
    {
        return new FallbackNominationBundleService(
            store,
            FallbackNominationBundleRules.DefaultFallbackNominationBundleValidator(),
            clock,
            new List<string>(),
            FallbackNominationBundleEngine.DefaultFallbackNominationBundleEngine(flags));
    }
}
