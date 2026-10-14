namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves statement of work batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record StatementOfWorkBatchWorkflow(IStatementOfWorkBatchStore Store, StatementOfWorkBatchValidator Validator, IClock Clock, List<string> Journal, StatementOfWorkBatchEngine Engine)
{
    public static StatementOfWorkBatchWorkflow NewStatementOfWorkBatchWorkflow(IStatementOfWorkBatchStore store, IClock clock, Registry flags)
    {
        return new StatementOfWorkBatchWorkflow(
            store,
            StatementOfWorkBatchRules.DefaultStatementOfWorkBatchValidator(),
            clock,
            new List<string>(),
            StatementOfWorkBatchEngine.DefaultStatementOfWorkBatchEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkBatchStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkBatchStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work batch {code} not found");
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
