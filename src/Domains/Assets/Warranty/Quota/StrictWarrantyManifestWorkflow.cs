namespace Acme.Workforce.Assets.Warranty.Quota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict warranty manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictWarrantyManifestWorkflow(IStrictWarrantyManifestStore Store, StrictWarrantyManifestValidator Validator, IClock Clock, List<string> Journal)
{
    public static StrictWarrantyManifestWorkflow NewStrictWarrantyManifestWorkflow(IStrictWarrantyManifestStore store, IClock clock)
    {
        return new StrictWarrantyManifestWorkflow(
            store,
            StrictWarrantyManifestRules.DefaultStrictWarrantyManifestValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict warranty manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictWarrantyManifestStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict warranty manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictWarrantyManifestStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.UpdatedAt > cutoff)
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
