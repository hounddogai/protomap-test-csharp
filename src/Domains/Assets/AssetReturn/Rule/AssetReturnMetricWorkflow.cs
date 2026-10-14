namespace Acme.Workforce.Assets.AssetReturn.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves asset return metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnMetricCoordinator(IAssetReturnMetricStore Store, AssetReturnMetricValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnMetricStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnMetricStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of asset return metric workflow.</summary>
public static class AssetReturnMetricWorkflow
{
    public static AssetReturnMetricCoordinator NewAssetReturnMetricCoordinator(IAssetReturnMetricStore store, IClock clock)
    {
        return new AssetReturnMetricCoordinator(
            store,
            AssetReturnMetricRules.DefaultAssetReturnMetricValidator(),
            clock,
            new List<string>());
    }
}
