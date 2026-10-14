namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary equity grant digests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryEquityGrantDigestWorkflow(IPrimaryEquityGrantDigestStore Store, PrimaryEquityGrantDigestValidator Validator, IClock Clock, List<string> Journal)
{
    public static PrimaryEquityGrantDigestWorkflow NewPrimaryEquityGrantDigestWorkflow(IPrimaryEquityGrantDigestStore store, IClock clock)
    {
        return new PrimaryEquityGrantDigestWorkflow(
            store,
            PrimaryEquityGrantDigestRules.DefaultPrimaryEquityGrantDigestValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary equity grant digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryEquityGrantDigestStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary equity grant digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryEquityGrantDigestStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
