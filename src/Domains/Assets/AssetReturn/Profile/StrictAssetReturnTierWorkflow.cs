namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict asset return tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictAssetReturnTierService(IStrictAssetReturnTierStore Store, StrictAssetReturnTierValidator Validator, IClock Clock, List<string> Journal, StrictAssetReturnTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict asset return tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictAssetReturnTierStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict asset return tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictAssetReturnTierStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict asset return tier {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of strict asset return tier workflow.</summary>
public static class StrictAssetReturnTierWorkflow
{
    public static StrictAssetReturnTierService NewStrictAssetReturnTierService(IStrictAssetReturnTierStore store, IClock clock, Registry flags)
    {
        return new StrictAssetReturnTierService(
            store,
            StrictAssetReturnTierRules.DefaultStrictAssetReturnTierValidator(),
            clock,
            new List<string>(),
            StrictAssetReturnTierEngine.DefaultStrictAssetReturnTierEngine(flags));
    }
}
