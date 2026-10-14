namespace Acme.Workforce.Absence.ReturnPlan.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves return plan quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReturnPlanQuotaManager(IReturnPlanQuotaStore Store, ReturnPlanQuotaValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReturnPlanQuotaStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReturnPlanQuotaStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of return plan quota workflow.</summary>
public static class ReturnPlanQuotaWorkflow
{
    public static ReturnPlanQuotaManager NewReturnPlanQuotaManager(IReturnPlanQuotaStore store, IClock clock)
    {
        return new ReturnPlanQuotaManager(
            store,
            ReturnPlanQuotaRules.DefaultReturnPlanQuotaValidator(),
            clock,
            new List<string>());
    }
}
