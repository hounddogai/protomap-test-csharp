namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves statement of work checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StatementOfWorkCheckpointDesk(IStatementOfWorkCheckpointStore Store, StatementOfWorkCheckpointValidator Validator, IClock Clock, List<string> Journal, StatementOfWorkCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkCheckpointStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkCheckpointStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work checkpoint {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of statement of work checkpoint workflow.</summary>
public static class StatementOfWorkCheckpointWorkflow
{
    public static StatementOfWorkCheckpointDesk NewStatementOfWorkCheckpointDesk(IStatementOfWorkCheckpointStore store, IClock clock, Registry flags)
    {
        return new StatementOfWorkCheckpointDesk(
            store,
            StatementOfWorkCheckpointRules.DefaultStatementOfWorkCheckpointValidator(),
            clock,
            new List<string>(),
            StatementOfWorkCheckpointEngine.DefaultStatementOfWorkCheckpointEngine(flags));
    }
}
