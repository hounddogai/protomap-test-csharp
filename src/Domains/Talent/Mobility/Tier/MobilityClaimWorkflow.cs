namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves mobility claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record MobilityClaimManager(IMobilityClaimStore Store, MobilityClaimValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MobilityClaimStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MobilityClaimStatus.Archived);
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

/// <summary>Functions of mobility claim workflow.</summary>
public static class MobilityClaimWorkflow
{
    public static MobilityClaimManager NewMobilityClaimManager(IMobilityClaimStore store, IClock clock)
    {
        return new MobilityClaimManager(
            store,
            MobilityClaimRules.DefaultMobilityClaimValidator(),
            clock,
            new List<string>());
    }
}
