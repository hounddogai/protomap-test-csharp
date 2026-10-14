namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient bonus pool claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientBonusPoolClaimService(ILenientBonusPoolClaimStore Store, LenientBonusPoolClaimValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient bonus pool claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientBonusPoolClaimStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient bonus pool claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientBonusPoolClaimStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of lenient bonus pool claim workflow.</summary>
public static class LenientBonusPoolClaimWorkflow
{
    public static LenientBonusPoolClaimService NewLenientBonusPoolClaimService(ILenientBonusPoolClaimStore store, IClock clock)
    {
        return new LenientBonusPoolClaimService(
            store,
            LenientBonusPoolClaimRules.DefaultLenientBonusPoolClaimValidator(),
            clock,
            new List<string>());
    }
}
