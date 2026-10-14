namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback nomination targets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackNominationTargetManager(IFallbackNominationTargetStore Store, FallbackNominationTargetValidator Validator, IClock Clock, List<string> Journal, FallbackNominationTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback nomination target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackNominationTargetStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback nomination target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackNominationTargetStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback nomination target {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of fallback nomination target workflow.</summary>
public static class FallbackNominationTargetWorkflow
{
    public static FallbackNominationTargetManager NewFallbackNominationTargetManager(IFallbackNominationTargetStore store, IClock clock, Registry flags)
    {
        return new FallbackNominationTargetManager(
            store,
            FallbackNominationTargetRules.DefaultFallbackNominationTargetValidator(),
            clock,
            new List<string>(),
            FallbackNominationTargetEngine.DefaultFallbackNominationTargetEngine(flags));
    }
}
