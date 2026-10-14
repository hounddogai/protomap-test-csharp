namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves succession plan manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record SuccessionPlanManifestManager(ISuccessionPlanManifestStore Store, SuccessionPlanManifestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SuccessionPlanManifestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SuccessionPlanManifestStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of succession plan manifest workflow.</summary>
public static class SuccessionPlanManifestWorkflow
{
    public static SuccessionPlanManifestManager NewSuccessionPlanManifestManager(ISuccessionPlanManifestStore store, IClock clock)
    {
        return new SuccessionPlanManifestManager(
            store,
            SuccessionPlanManifestRules.DefaultSuccessionPlanManifestValidator(),
            clock,
            new List<string>());
    }
}
