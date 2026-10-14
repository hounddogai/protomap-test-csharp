namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves asset return baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnBaselineDesk(IAssetReturnBaselineStore Store, AssetReturnBaselineValidator Validator, IClock Clock, List<string> Journal, AssetReturnBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnBaselineStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnBaselineStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return baseline {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of asset return baseline workflow.</summary>
public static class AssetReturnBaselineWorkflow
{
    public static AssetReturnBaselineDesk NewAssetReturnBaselineDesk(IAssetReturnBaselineStore store, IClock clock, Registry flags)
    {
        return new AssetReturnBaselineDesk(
            store,
            AssetReturnBaselineRules.DefaultAssetReturnBaselineValidator(),
            clock,
            new List<string>(),
            AssetReturnBaselineEngine.DefaultAssetReturnBaselineEngine(flags));
    }
}
