namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered warranty manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredWarrantyManifestService(ITieredWarrantyManifestStore Store, TieredWarrantyManifestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered warranty manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredWarrantyManifestStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered warranty manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredWarrantyManifestStatus.Withdrawn);
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

/// <summary>Functions of tiered warranty manifest workflow.</summary>
public static class TieredWarrantyManifestWorkflow
{
    public static TieredWarrantyManifestService NewTieredWarrantyManifestService(ITieredWarrantyManifestStore store, IClock clock)
    {
        return new TieredWarrantyManifestService(
            store,
            TieredWarrantyManifestRules.DefaultTieredWarrantyManifestValidator(),
            clock,
            new List<string>());
    }
}
