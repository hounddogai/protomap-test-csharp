namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves statement of work audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record StatementOfWorkAuditDesk(IStatementOfWorkAuditStore Store, StatementOfWorkAuditValidator Validator, IClock Clock, List<string> Journal, StatementOfWorkAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkAuditStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkAuditStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work audit {code} not found");
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

/// <summary>Functions of statement of work audit workflow.</summary>
public static class StatementOfWorkAuditWorkflow
{
    public static StatementOfWorkAuditDesk NewStatementOfWorkAuditDesk(IStatementOfWorkAuditStore store, IClock clock, Registry flags)
    {
        return new StatementOfWorkAuditDesk(
            store,
            StatementOfWorkAuditRules.DefaultStatementOfWorkAuditValidator(),
            clock,
            new List<string>(),
            StatementOfWorkAuditEngine.DefaultStatementOfWorkAuditEngine(flags));
    }
}
