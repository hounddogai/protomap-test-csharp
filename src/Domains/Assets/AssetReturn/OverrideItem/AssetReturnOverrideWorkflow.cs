namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves asset return overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnOverrideManager(IAssetReturnOverrideStore Store, AssetReturnOverrideValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnOverrideStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnOverrideStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of asset return override workflow.</summary>
public static class AssetReturnOverrideWorkflow
{
    public static AssetReturnOverrideManager NewAssetReturnOverrideManager(IAssetReturnOverrideStore store, IClock clock)
    {
        return new AssetReturnOverrideManager(
            store,
            AssetReturnOverrideRules.DefaultAssetReturnOverrideValidator(),
            clock,
            new List<string>());
    }
}
