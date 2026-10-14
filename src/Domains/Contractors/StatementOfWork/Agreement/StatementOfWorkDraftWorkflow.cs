namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves statement of work drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record StatementOfWorkDraftDesk(IStatementOfWorkDraftStore Store, StatementOfWorkDraftValidator Validator, IClock Clock, List<string> Journal, StatementOfWorkDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkDraftStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkDraftStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work draft {code} not found");
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

/// <summary>Functions of statement of work draft workflow.</summary>
public static class StatementOfWorkDraftWorkflow
{
    public static StatementOfWorkDraftDesk NewStatementOfWorkDraftDesk(IStatementOfWorkDraftStore store, IClock clock)
    {
        return new StatementOfWorkDraftDesk(
            store,
            StatementOfWorkDraftRules.DefaultStatementOfWorkDraftValidator(),
            clock,
            new List<string>(),
            StatementOfWorkDraftEngine.DefaultStatementOfWorkDraftEngine());
    }
}
