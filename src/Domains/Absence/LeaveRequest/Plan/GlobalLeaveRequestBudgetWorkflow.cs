namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global leave request budgets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record GlobalLeaveRequestBudgetCoordinator(IGlobalLeaveRequestBudgetStore Store, GlobalLeaveRequestBudgetValidator Validator, IClock Clock, List<string> Journal, GlobalLeaveRequestBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global leave request budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalLeaveRequestBudgetStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global leave request budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalLeaveRequestBudgetStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global leave request budget {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of global leave request budget workflow.</summary>
public static class GlobalLeaveRequestBudgetWorkflow
{
    public static GlobalLeaveRequestBudgetCoordinator NewGlobalLeaveRequestBudgetCoordinator(IGlobalLeaveRequestBudgetStore store, IClock clock, Registry flags)
    {
        return new GlobalLeaveRequestBudgetCoordinator(
            store,
            GlobalLeaveRequestBudgetRules.DefaultGlobalLeaveRequestBudgetValidator(),
            clock,
            new List<string>(),
            GlobalLeaveRequestBudgetEngine.DefaultGlobalLeaveRequestBudgetEngine(flags));
    }
}
