namespace Acme.Workforce.Assets.AssetReturn.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves asset return digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record AssetReturnDigestDesk(IAssetReturnDigestStore Store, AssetReturnDigestValidator Validator, IClock Clock, List<string> Journal, AssetReturnDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AssetReturnDigestStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AssetReturnDigestStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"asset return digest {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of asset return digest workflow.</summary>
public static class AssetReturnDigestWorkflow
{
    public static AssetReturnDigestDesk NewAssetReturnDigestDesk(IAssetReturnDigestStore store, IClock clock, Registry flags)
    {
        return new AssetReturnDigestDesk(
            store,
            AssetReturnDigestRules.DefaultAssetReturnDigestValidator(),
            clock,
            new List<string>(),
            AssetReturnDigestEngine.DefaultAssetReturnDigestEngine(flags));
    }
}
