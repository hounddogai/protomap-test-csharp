namespace Acme.Workforce.Absence.Balance.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional balance digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalBalanceDigestService(IRegionalBalanceDigestStore Store, RegionalBalanceDigestValidator Validator, IClock Clock, List<string> Journal, RegionalBalanceDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional balance digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalBalanceDigestStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional balance digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalBalanceDigestStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional balance digest {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of regional balance digest workflow.</summary>
public static class RegionalBalanceDigestWorkflow
{
    public static RegionalBalanceDigestService NewRegionalBalanceDigestService(IRegionalBalanceDigestStore store, IClock clock, Registry flags)
    {
        return new RegionalBalanceDigestService(
            store,
            RegionalBalanceDigestRules.DefaultRegionalBalanceDigestValidator(),
            clock,
            new List<string>(),
            RegionalBalanceDigestEngine.DefaultRegionalBalanceDigestEngine(flags));
    }
}
