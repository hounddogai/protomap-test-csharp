namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default return plan tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultReturnPlanTierService(IDefaultReturnPlanTierStore Store, DefaultReturnPlanTierValidator Validator, IClock Clock, List<string> Journal, DefaultReturnPlanTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default return plan tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultReturnPlanTierStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default return plan tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultReturnPlanTierStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default return plan tier {code} not found");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of default return plan tier workflow.</summary>
public static class DefaultReturnPlanTierWorkflow
{
    public static DefaultReturnPlanTierService NewDefaultReturnPlanTierService(IDefaultReturnPlanTierStore store, IClock clock, Registry flags)
    {
        return new DefaultReturnPlanTierService(
            store,
            DefaultReturnPlanTierRules.DefaultDefaultReturnPlanTierValidator(),
            clock,
            new List<string>(),
            DefaultReturnPlanTierEngine.DefaultDefaultReturnPlanTierEngine(flags));
    }
}
