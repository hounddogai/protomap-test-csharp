namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional span digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalSpanDigestService(IRegionalSpanDigestStore Store, RegionalSpanDigestValidator Validator, IClock Clock, List<string> Journal, RegionalSpanDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional span digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalSpanDigestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional span digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalSpanDigestStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional span digest {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of regional span digest workflow.</summary>
public static class RegionalSpanDigestWorkflow
{
    public static RegionalSpanDigestService NewRegionalSpanDigestService(IRegionalSpanDigestStore store, IClock clock)
    {
        return new RegionalSpanDigestService(
            store,
            RegionalSpanDigestRules.DefaultRegionalSpanDigestValidator(),
            clock,
            new List<string>(),
            RegionalSpanDigestEngine.DefaultRegionalSpanDigestEngine());
    }
}
