namespace Acme.Workforce.Absence.ReturnPlan.PlanLedger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient return plan manifests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientReturnPlanManifestCoordinator(ILenientReturnPlanManifestStore Store, LenientReturnPlanManifestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient return plan manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientReturnPlanManifestStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient return plan manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientReturnPlanManifestStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of lenient return plan manifest workflow.</summary>
public static class LenientReturnPlanManifestWorkflow
{
    public static LenientReturnPlanManifestCoordinator NewLenientReturnPlanManifestCoordinator(ILenientReturnPlanManifestStore store, IClock clock)
    {
        return new LenientReturnPlanManifestCoordinator(
            store,
            LenientReturnPlanManifestRules.DefaultLenientReturnPlanManifestValidator(),
            clock,
            new List<string>());
    }
}
