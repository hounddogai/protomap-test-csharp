namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional statement of work milestones through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalStatementOfWorkMilestoneDesk(IRegionalStatementOfWorkMilestoneStore Store, RegionalStatementOfWorkMilestoneValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional statement of work milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalStatementOfWorkMilestoneStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional statement of work milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalStatementOfWorkMilestoneStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public int Scan(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of regional statement of work milestone workflow.</summary>
public static class RegionalStatementOfWorkMilestoneWorkflow
{
    public static RegionalStatementOfWorkMilestoneDesk NewRegionalStatementOfWorkMilestoneDesk(IRegionalStatementOfWorkMilestoneStore store, IClock clock)
    {
        return new RegionalStatementOfWorkMilestoneDesk(
            store,
            RegionalStatementOfWorkMilestoneRules.DefaultRegionalStatementOfWorkMilestoneValidator(),
            clock,
            new List<string>());
    }
}
