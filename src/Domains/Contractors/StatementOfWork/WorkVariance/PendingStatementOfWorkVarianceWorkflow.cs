namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending statement of work variances through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingStatementOfWorkVarianceManager(IPendingStatementOfWorkVarianceStore Store, PendingStatementOfWorkVarianceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending statement of work variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingStatementOfWorkVarianceStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending statement of work variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingStatementOfWorkVarianceStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of pending statement of work variance workflow.</summary>
public static class PendingStatementOfWorkVarianceWorkflow
{
    public static PendingStatementOfWorkVarianceManager NewPendingStatementOfWorkVarianceManager(IPendingStatementOfWorkVarianceStore store, IClock clock)
    {
        return new PendingStatementOfWorkVarianceManager(
            store,
            PendingStatementOfWorkVarianceRules.DefaultPendingStatementOfWorkVarianceValidator(),
            clock,
            new List<string>());
    }
}
