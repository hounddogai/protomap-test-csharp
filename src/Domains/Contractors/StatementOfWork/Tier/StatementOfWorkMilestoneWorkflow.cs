namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves statement of work milestones through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StatementOfWorkMilestoneService(IStatementOfWorkMilestoneStore Store, StatementOfWorkMilestoneValidator Validator, IClock Clock, List<string> Journal, StatementOfWorkMilestoneEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkMilestoneStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkMilestoneStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work milestone {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of statement of work milestone workflow.</summary>
public static class StatementOfWorkMilestoneWorkflow
{
    public static StatementOfWorkMilestoneService NewStatementOfWorkMilestoneService(IStatementOfWorkMilestoneStore store, IClock clock, Registry flags)
    {
        return new StatementOfWorkMilestoneService(
            store,
            StatementOfWorkMilestoneRules.DefaultStatementOfWorkMilestoneValidator(),
            clock,
            new List<string>(),
            StatementOfWorkMilestoneEngine.DefaultStatementOfWorkMilestoneEngine(flags));
    }
}
