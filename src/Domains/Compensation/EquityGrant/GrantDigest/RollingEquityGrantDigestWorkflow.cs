namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling equity grant digests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingEquityGrantDigestManager(IRollingEquityGrantDigestStore Store, RollingEquityGrantDigestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling equity grant digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingEquityGrantDigestStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling equity grant digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingEquityGrantDigestStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of rolling equity grant digest workflow.</summary>
public static class RollingEquityGrantDigestWorkflow
{
    public static RollingEquityGrantDigestManager NewRollingEquityGrantDigestManager(IRollingEquityGrantDigestStore store, IClock clock)
    {
        return new RollingEquityGrantDigestManager(
            store,
            RollingEquityGrantDigestRules.DefaultRollingEquityGrantDigestValidator(),
            clock,
            new List<string>());
    }
}
