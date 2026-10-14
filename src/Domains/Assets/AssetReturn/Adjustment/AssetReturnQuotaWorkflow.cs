namespace Acme.Workforce.Assets.AssetReturn.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves asset return quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnQuotaCoordinator(IAssetReturnQuotaStore Store, AssetReturnQuotaValidator Validator, IClock Clock, List<string> Journal, AssetReturnQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnQuotaStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnQuotaStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return quota {code} not found");
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

/// <summary>Functions of asset return quota workflow.</summary>
public static class AssetReturnQuotaWorkflow
{
    public static AssetReturnQuotaCoordinator NewAssetReturnQuotaCoordinator(IAssetReturnQuotaStore store, IClock clock)
    {
        return new AssetReturnQuotaCoordinator(
            store,
            AssetReturnQuotaRules.DefaultAssetReturnQuotaValidator(),
            clock,
            new List<string>(),
            AssetReturnQuotaEngine.DefaultAssetReturnQuotaEngine());
    }
}
