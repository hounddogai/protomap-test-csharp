namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves statement of work targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record StatementOfWorkTargetDesk(IStatementOfWorkTargetStore Store, StatementOfWorkTargetValidator Validator, IClock Clock, List<string> Journal, StatementOfWorkTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkTargetStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkTargetStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work target {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of statement of work target workflow.</summary>
public static class StatementOfWorkTargetWorkflow
{
    public static StatementOfWorkTargetDesk NewStatementOfWorkTargetDesk(IStatementOfWorkTargetStore store, IClock clock)
    {
        return new StatementOfWorkTargetDesk(
            store,
            StatementOfWorkTargetRules.DefaultStatementOfWorkTargetValidator(),
            clock,
            new List<string>(),
            StatementOfWorkTargetEngine.DefaultStatementOfWorkTargetEngine());
    }
}
