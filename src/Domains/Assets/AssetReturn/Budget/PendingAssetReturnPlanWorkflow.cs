namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending asset return plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingAssetReturnPlanService(IPendingAssetReturnPlanStore Store, PendingAssetReturnPlanValidator Validator, IClock Clock, List<string> Journal, PendingAssetReturnPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending asset return plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingAssetReturnPlanStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending asset return plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingAssetReturnPlanStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending asset return plan {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of pending asset return plan workflow.</summary>
public static class PendingAssetReturnPlanWorkflow
{
    public static PendingAssetReturnPlanService NewPendingAssetReturnPlanService(IPendingAssetReturnPlanStore store, IClock clock)
    {
        return new PendingAssetReturnPlanService(
            store,
            PendingAssetReturnPlanRules.DefaultPendingAssetReturnPlanValidator(),
            clock,
            new List<string>(),
            PendingAssetReturnPlanEngine.DefaultPendingAssetReturnPlanEngine());
    }
}
