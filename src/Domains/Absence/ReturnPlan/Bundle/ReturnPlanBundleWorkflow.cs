namespace Acme.Workforce.Absence.ReturnPlan.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves return plan bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReturnPlanBundleCoordinator(IReturnPlanBundleStore Store, ReturnPlanBundleValidator Validator, IClock Clock, List<string> Journal, ReturnPlanBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReturnPlanBundleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReturnPlanBundleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan bundle {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of return plan bundle workflow.</summary>
public static class ReturnPlanBundleWorkflow
{
    public static ReturnPlanBundleCoordinator NewReturnPlanBundleCoordinator(IReturnPlanBundleStore store, IClock clock)
    {
        return new ReturnPlanBundleCoordinator(
            store,
            ReturnPlanBundleRules.DefaultReturnPlanBundleValidator(),
            clock,
            new List<string>(),
            ReturnPlanBundleEngine.DefaultReturnPlanBundleEngine());
    }
}
