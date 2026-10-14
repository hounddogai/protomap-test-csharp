namespace Acme.Workforce.Assets.AssetReturn.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual asset return tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualAssetReturnTierManager(IManualAssetReturnTierStore Store, ManualAssetReturnTierValidator Validator, IClock Clock, List<string> Journal, ManualAssetReturnTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual asset return tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualAssetReturnTierStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual asset return tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualAssetReturnTierStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual asset return tier {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of manual asset return tier workflow.</summary>
public static class ManualAssetReturnTierWorkflow
{
    public static ManualAssetReturnTierManager NewManualAssetReturnTierManager(IManualAssetReturnTierStore store, IClock clock)
    {
        return new ManualAssetReturnTierManager(
            store,
            ManualAssetReturnTierRules.DefaultManualAssetReturnTierValidator(),
            clock,
            new List<string>(),
            ManualAssetReturnTierEngine.DefaultManualAssetReturnTierEngine());
    }
}
