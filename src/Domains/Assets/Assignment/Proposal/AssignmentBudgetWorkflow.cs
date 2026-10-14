namespace Acme.Workforce.Assets.Assignment.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves assignment budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssignmentBudgetService(IAssignmentBudgetStore Store, AssignmentBudgetValidator Validator, IClock Clock, List<string> Journal, AssignmentBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssignmentBudgetStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssignmentBudgetStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"assignment budget {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of assignment budget workflow.</summary>
public static class AssignmentBudgetWorkflow
{
    public static AssignmentBudgetService NewAssignmentBudgetService(IAssignmentBudgetStore store, IClock clock, Registry flags)
    {
        return new AssignmentBudgetService(
            store,
            AssignmentBudgetRules.DefaultAssignmentBudgetValidator(),
            clock,
            new List<string>(),
            AssignmentBudgetEngine.DefaultAssignmentBudgetEngine(flags));
    }
}
