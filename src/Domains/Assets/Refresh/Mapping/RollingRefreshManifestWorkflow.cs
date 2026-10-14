namespace Acme.Workforce.Assets.Refresh.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling refresh manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingRefreshManifestCoordinator(IRollingRefreshManifestStore Store, RollingRefreshManifestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling refresh manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingRefreshManifestStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling refresh manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingRefreshManifestStatus.Cancelled);
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

/// <summary>Functions of rolling refresh manifest workflow.</summary>
public static class RollingRefreshManifestWorkflow
{
    public static RollingRefreshManifestCoordinator NewRollingRefreshManifestCoordinator(IRollingRefreshManifestStore store, IClock clock)
    {
        return new RollingRefreshManifestCoordinator(
            store,
            RollingRefreshManifestRules.DefaultRollingRefreshManifestValidator(),
            clock,
            new List<string>());
    }
}
