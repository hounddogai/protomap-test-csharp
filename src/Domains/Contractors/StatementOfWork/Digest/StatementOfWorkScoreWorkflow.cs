namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves statement of work scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record StatementOfWorkScoreDesk(IStatementOfWorkScoreStore Store, StatementOfWorkScoreValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkScoreStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkScoreStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of statement of work score workflow.</summary>
public static class StatementOfWorkScoreWorkflow
{
    public static StatementOfWorkScoreDesk NewStatementOfWorkScoreDesk(IStatementOfWorkScoreStore store, IClock clock)
    {
        return new StatementOfWorkScoreDesk(
            store,
            StatementOfWorkScoreRules.DefaultStatementOfWorkScoreValidator(),
            clock,
            new List<string>());
    }
}
