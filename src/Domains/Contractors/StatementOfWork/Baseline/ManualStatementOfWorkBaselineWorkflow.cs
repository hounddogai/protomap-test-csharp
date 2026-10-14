namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual statement of work baselines through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualStatementOfWorkBaselineService(IManualStatementOfWorkBaselineStore Store, ManualStatementOfWorkBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual statement of work baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualStatementOfWorkBaselineStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual statement of work baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualStatementOfWorkBaselineStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of manual statement of work baseline workflow.</summary>
public static class ManualStatementOfWorkBaselineWorkflow
{
    public static ManualStatementOfWorkBaselineService NewManualStatementOfWorkBaselineService(IManualStatementOfWorkBaselineStore store, IClock clock)
    {
        return new ManualStatementOfWorkBaselineService(
            store,
            ManualStatementOfWorkBaselineRules.DefaultManualStatementOfWorkBaselineValidator(),
            clock,
            new List<string>());
    }
}
