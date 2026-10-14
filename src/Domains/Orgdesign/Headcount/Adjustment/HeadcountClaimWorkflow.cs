namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves headcount claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountClaimCoordinator(IHeadcountClaimStore Store, HeadcountClaimValidator Validator, IClock Clock, List<string> Journal, HeadcountClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountClaimStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountClaimStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount claim {code} not found");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of headcount claim workflow.</summary>
public static class HeadcountClaimWorkflow
{
    public static HeadcountClaimCoordinator NewHeadcountClaimCoordinator(IHeadcountClaimStore store, IClock clock, Registry flags)
    {
        return new HeadcountClaimCoordinator(
            store,
            HeadcountClaimRules.DefaultHeadcountClaimValidator(),
            clock,
            new List<string>(),
            HeadcountClaimEngine.DefaultHeadcountClaimEngine(flags));
    }
}
