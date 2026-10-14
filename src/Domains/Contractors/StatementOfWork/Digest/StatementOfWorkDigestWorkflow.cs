namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves statement of work digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record StatementOfWorkDigestDesk(IStatementOfWorkDigestStore Store, StatementOfWorkDigestValidator Validator, IClock Clock, List<string> Journal, StatementOfWorkDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkDigestStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkDigestStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work digest {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of statement of work digest workflow.</summary>
public static class StatementOfWorkDigestWorkflow
{
    public static StatementOfWorkDigestDesk NewStatementOfWorkDigestDesk(IStatementOfWorkDigestStore store, IClock clock)
    {
        return new StatementOfWorkDigestDesk(
            store,
            StatementOfWorkDigestRules.DefaultStatementOfWorkDigestValidator(),
            clock,
            new List<string>(),
            StatementOfWorkDigestEngine.DefaultStatementOfWorkDigestEngine());
    }
}
