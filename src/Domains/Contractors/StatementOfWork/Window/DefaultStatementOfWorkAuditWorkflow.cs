namespace Acme.Workforce.Contractors.StatementOfWork.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default statement of work audits through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultStatementOfWorkAuditWorkflow(IDefaultStatementOfWorkAuditStore Store, DefaultStatementOfWorkAuditValidator Validator, IClock Clock, List<string> Journal, DefaultStatementOfWorkAuditEngine Engine)
{
    public static DefaultStatementOfWorkAuditWorkflow NewDefaultStatementOfWorkAuditWorkflow(IDefaultStatementOfWorkAuditStore store, IClock clock, Registry flags)
    {
        return new DefaultStatementOfWorkAuditWorkflow(
            store,
            DefaultStatementOfWorkAuditRules.DefaultDefaultStatementOfWorkAuditValidator(),
            clock,
            new List<string>(),
            DefaultStatementOfWorkAuditEngine.DefaultDefaultStatementOfWorkAuditEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default statement of work audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultStatementOfWorkAuditStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default statement of work audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultStatementOfWorkAuditStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default statement of work audit {code} not found");
        }
        var result = Engine.Evaluate(item);
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
