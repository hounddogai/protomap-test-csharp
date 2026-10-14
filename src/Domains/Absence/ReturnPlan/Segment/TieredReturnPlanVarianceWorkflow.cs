namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered return plan variances through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredReturnPlanVarianceDesk(ITieredReturnPlanVarianceStore Store, TieredReturnPlanVarianceValidator Validator, IClock Clock, List<string> Journal, TieredReturnPlanVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered return plan variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredReturnPlanVarianceStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered return plan variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredReturnPlanVarianceStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered return plan variance {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of tiered return plan variance workflow.</summary>
public static class TieredReturnPlanVarianceWorkflow
{
    public static TieredReturnPlanVarianceDesk NewTieredReturnPlanVarianceDesk(ITieredReturnPlanVarianceStore store, IClock clock)
    {
        return new TieredReturnPlanVarianceDesk(
            store,
            TieredReturnPlanVarianceRules.DefaultTieredReturnPlanVarianceValidator(),
            clock,
            new List<string>(),
            TieredReturnPlanVarianceEngine.DefaultTieredReturnPlanVarianceEngine());
    }
}
