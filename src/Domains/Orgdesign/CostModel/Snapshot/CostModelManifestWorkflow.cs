namespace Acme.Workforce.Orgdesign.CostModel.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves cost model manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record CostModelManifestService(ICostModelManifestStore Store, CostModelManifestValidator Validator, IClock Clock, List<string> Journal, CostModelManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CostModelManifestStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CostModelManifestStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model manifest {code} not found");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of cost model manifest workflow.</summary>
public static class CostModelManifestWorkflow
{
    public static CostModelManifestService NewCostModelManifestService(ICostModelManifestStore store, IClock clock, Registry flags)
    {
        return new CostModelManifestService(
            store,
            CostModelManifestRules.DefaultCostModelManifestValidator(),
            clock,
            new List<string>(),
            CostModelManifestEngine.DefaultCostModelManifestEngine(flags));
    }
}
