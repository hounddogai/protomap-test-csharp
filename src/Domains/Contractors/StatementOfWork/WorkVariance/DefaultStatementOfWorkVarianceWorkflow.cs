namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default statement of work variances through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultStatementOfWorkVarianceService(IDefaultStatementOfWorkVarianceStore Store, DefaultStatementOfWorkVarianceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default statement of work variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultStatementOfWorkVarianceStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default statement of work variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultStatementOfWorkVarianceStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of default statement of work variance workflow.</summary>
public static class DefaultStatementOfWorkVarianceWorkflow
{
    public static DefaultStatementOfWorkVarianceService NewDefaultStatementOfWorkVarianceService(IDefaultStatementOfWorkVarianceStore store, IClock clock)
    {
        return new DefaultStatementOfWorkVarianceService(
            store,
            DefaultStatementOfWorkVarianceRules.DefaultDefaultStatementOfWorkVarianceValidator(),
            clock,
            new List<string>());
    }
}
