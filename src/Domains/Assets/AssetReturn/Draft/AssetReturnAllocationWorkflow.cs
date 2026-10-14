namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves asset return allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnAllocationManager(IAssetReturnAllocationStore Store, AssetReturnAllocationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnAllocationStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnAllocationStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of asset return allocation workflow.</summary>
public static class AssetReturnAllocationWorkflow
{
    public static AssetReturnAllocationManager NewAssetReturnAllocationManager(IAssetReturnAllocationStore store, IClock clock)
    {
        return new AssetReturnAllocationManager(
            store,
            AssetReturnAllocationRules.DefaultAssetReturnAllocationValidator(),
            clock,
            new List<string>());
    }
}
