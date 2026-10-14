namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves return plan estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReturnPlanEstimateManager(IReturnPlanEstimateStore Store, ReturnPlanEstimateValidator Validator, IClock Clock, List<string> Journal, ReturnPlanEstimateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReturnPlanEstimateStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReturnPlanEstimateStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan estimate {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of return plan estimate workflow.</summary>
public static class ReturnPlanEstimateWorkflow
{
    public static ReturnPlanEstimateManager NewReturnPlanEstimateManager(IReturnPlanEstimateStore store, IClock clock, Registry flags)
    {
        return new ReturnPlanEstimateManager(
            store,
            ReturnPlanEstimateRules.DefaultReturnPlanEstimateValidator(),
            clock,
            new List<string>(),
            ReturnPlanEstimateEngine.DefaultReturnPlanEstimateEngine(flags));
    }
}
