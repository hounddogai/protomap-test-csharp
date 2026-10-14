namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves statement of work schedules through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StatementOfWorkScheduleService(IStatementOfWorkScheduleStore Store, StatementOfWorkScheduleValidator Validator, IClock Clock, List<string> Journal, StatementOfWorkScheduleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkScheduleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkScheduleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work schedule {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of statement of work schedule workflow.</summary>
public static class StatementOfWorkScheduleWorkflow
{
    public static StatementOfWorkScheduleService NewStatementOfWorkScheduleService(IStatementOfWorkScheduleStore store, IClock clock, Registry flags)
    {
        return new StatementOfWorkScheduleService(
            store,
            StatementOfWorkScheduleRules.DefaultStatementOfWorkScheduleValidator(),
            clock,
            new List<string>(),
            StatementOfWorkScheduleEngine.DefaultStatementOfWorkScheduleEngine(flags));
    }
}
