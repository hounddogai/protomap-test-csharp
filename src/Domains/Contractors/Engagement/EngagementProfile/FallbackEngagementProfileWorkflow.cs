namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback engagement profiles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackEngagementProfileManager(IFallbackEngagementProfileStore Store, FallbackEngagementProfileValidator Validator, IClock Clock, List<string> Journal, FallbackEngagementProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback engagement profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackEngagementProfileStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback engagement profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackEngagementProfileStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback engagement profile {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of fallback engagement profile workflow.</summary>
public static class FallbackEngagementProfileWorkflow
{
    public static FallbackEngagementProfileManager NewFallbackEngagementProfileManager(IFallbackEngagementProfileStore store, IClock clock, Registry flags)
    {
        return new FallbackEngagementProfileManager(
            store,
            FallbackEngagementProfileRules.DefaultFallbackEngagementProfileValidator(),
            clock,
            new List<string>(),
            FallbackEngagementProfileEngine.DefaultFallbackEngagementProfileEngine(flags));
    }
}
