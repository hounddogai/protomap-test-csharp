namespace Acme.Workforce.Absence.ReturnPlan.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default return plan manifests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultReturnPlanManifestManager(IDefaultReturnPlanManifestStore Store, DefaultReturnPlanManifestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default return plan manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultReturnPlanManifestStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default return plan manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultReturnPlanManifestStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of default return plan manifest workflow.</summary>
public static class DefaultReturnPlanManifestWorkflow
{
    public static DefaultReturnPlanManifestManager NewDefaultReturnPlanManifestManager(IDefaultReturnPlanManifestStore store, IClock clock)
    {
        return new DefaultReturnPlanManifestManager(
            store,
            DefaultReturnPlanManifestRules.DefaultDefaultReturnPlanManifestValidator(),
            clock,
            new List<string>());
    }
}
