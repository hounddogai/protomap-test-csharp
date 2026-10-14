namespace Acme.Workforce.Assets.AssetReturn.Quota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal asset return quotas through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalAssetReturnQuotaService(ISeasonalAssetReturnQuotaStore Store, SeasonalAssetReturnQuotaValidator Validator, IClock Clock, List<string> Journal, SeasonalAssetReturnQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal asset return quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalAssetReturnQuotaStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal asset return quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalAssetReturnQuotaStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal asset return quota {code} not found");
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

/// <summary>Functions of seasonal asset return quota workflow.</summary>
public static class SeasonalAssetReturnQuotaWorkflow
{
    public static SeasonalAssetReturnQuotaService NewSeasonalAssetReturnQuotaService(ISeasonalAssetReturnQuotaStore store, IClock clock)
    {
        return new SeasonalAssetReturnQuotaService(
            store,
            SeasonalAssetReturnQuotaRules.DefaultSeasonalAssetReturnQuotaValidator(),
            clock,
            new List<string>(),
            SeasonalAssetReturnQuotaEngine.DefaultSeasonalAssetReturnQuotaEngine());
    }
}
