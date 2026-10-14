namespace Acme.Workforce.Assets.AssetReturn.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict asset return manifests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictAssetReturnManifestService(IStrictAssetReturnManifestStore Store, StrictAssetReturnManifestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict asset return manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictAssetReturnManifestStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict asset return manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictAssetReturnManifestStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of strict asset return manifest workflow.</summary>
public static class StrictAssetReturnManifestWorkflow
{
    public static StrictAssetReturnManifestService NewStrictAssetReturnManifestService(IStrictAssetReturnManifestStore store, IClock clock)
    {
        return new StrictAssetReturnManifestService(
            store,
            StrictAssetReturnManifestRules.DefaultStrictAssetReturnManifestValidator(),
            clock,
            new List<string>());
    }
}
