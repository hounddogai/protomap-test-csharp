namespace Acme.Workforce.Absence.ReturnPlan.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves return plan schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReturnPlanScheduleService(IReturnPlanScheduleStore Store, ReturnPlanScheduleValidator Validator, IClock Clock, List<string> Journal, ReturnPlanScheduleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReturnPlanScheduleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReturnPlanScheduleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan schedule {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of return plan schedule workflow.</summary>
public static class ReturnPlanScheduleWorkflow
{
    public static ReturnPlanScheduleService NewReturnPlanScheduleService(IReturnPlanScheduleStore store, IClock clock, Registry flags)
    {
        return new ReturnPlanScheduleService(
            store,
            ReturnPlanScheduleRules.DefaultReturnPlanScheduleValidator(),
            clock,
            new List<string>(),
            ReturnPlanScheduleEngine.DefaultReturnPlanScheduleEngine(flags));
    }
}
