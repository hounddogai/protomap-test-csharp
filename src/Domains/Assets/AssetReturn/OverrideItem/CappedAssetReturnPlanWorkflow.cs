namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped asset return plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedAssetReturnPlanService(ICappedAssetReturnPlanStore Store, CappedAssetReturnPlanValidator Validator, IClock Clock, List<string> Journal, CappedAssetReturnPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped asset return plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedAssetReturnPlanStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped asset return plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedAssetReturnPlanStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped asset return plan {code} not found");
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

/// <summary>Functions of capped asset return plan workflow.</summary>
public static class CappedAssetReturnPlanWorkflow
{
    public static CappedAssetReturnPlanService NewCappedAssetReturnPlanService(ICappedAssetReturnPlanStore store, IClock clock, Registry flags)
    {
        return new CappedAssetReturnPlanService(
            store,
            CappedAssetReturnPlanRules.DefaultCappedAssetReturnPlanValidator(),
            clock,
            new List<string>(),
            CappedAssetReturnPlanEngine.DefaultCappedAssetReturnPlanEngine(flags));
    }
}
