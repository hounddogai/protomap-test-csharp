namespace Acme.Workforce.Contractors.StatementOfWork.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional statement of work digests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalStatementOfWorkDigestManager(IRegionalStatementOfWorkDigestStore Store, RegionalStatementOfWorkDigestValidator Validator, IClock Clock, List<string> Journal, RegionalStatementOfWorkDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional statement of work digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalStatementOfWorkDigestStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional statement of work digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalStatementOfWorkDigestStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional statement of work digest {code} not found");
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

/// <summary>Functions of regional statement of work digest workflow.</summary>
public static class RegionalStatementOfWorkDigestWorkflow
{
    public static RegionalStatementOfWorkDigestManager NewRegionalStatementOfWorkDigestManager(IRegionalStatementOfWorkDigestStore store, IClock clock)
    {
        return new RegionalStatementOfWorkDigestManager(
            store,
            RegionalStatementOfWorkDigestRules.DefaultRegionalStatementOfWorkDigestValidator(),
            clock,
            new List<string>(),
            RegionalStatementOfWorkDigestEngine.DefaultRegionalStatementOfWorkDigestEngine());
    }
}
