namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves return plan cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReturnPlanCycleWorkflow(IReturnPlanCycleStore Store, ReturnPlanCycleValidator Validator, IClock Clock, List<string> Journal)
{
    public static ReturnPlanCycleWorkflow NewReturnPlanCycleWorkflow(IReturnPlanCycleStore store, IClock clock)
    {
        return new ReturnPlanCycleWorkflow(
            store,
            ReturnPlanCycleRules.DefaultReturnPlanCycleValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReturnPlanCycleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReturnPlanCycleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
