namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary accrual plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryAccrualPlanManager(IPrimaryAccrualPlanStore Store, PrimaryAccrualPlanValidator Validator, IClock Clock, List<string> Journal, PrimaryAccrualPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary accrual plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryAccrualPlanStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary accrual plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryAccrualPlanStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary accrual plan {code} not found");
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

/// <summary>Functions of primary accrual plan workflow.</summary>
public static class PrimaryAccrualPlanWorkflow
{
    public static PrimaryAccrualPlanManager NewPrimaryAccrualPlanManager(IPrimaryAccrualPlanStore store, IClock clock)
    {
        return new PrimaryAccrualPlanManager(
            store,
            PrimaryAccrualPlanRules.DefaultPrimaryAccrualPlanValidator(),
            clock,
            new List<string>(),
            PrimaryAccrualPlanEngine.DefaultPrimaryAccrualPlanEngine());
    }
}
