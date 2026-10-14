namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves engagement manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record EngagementManifestCoordinator(IEngagementManifestStore Store, EngagementManifestValidator Validator, IClock Clock, List<string> Journal, EngagementManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EngagementManifestStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EngagementManifestStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement manifest {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of engagement manifest workflow.</summary>
public static class EngagementManifestWorkflow
{
    public static EngagementManifestCoordinator NewEngagementManifestCoordinator(IEngagementManifestStore store, IClock clock, Registry flags)
    {
        return new EngagementManifestCoordinator(
            store,
            EngagementManifestRules.DefaultEngagementManifestValidator(),
            clock,
            new List<string>(),
            EngagementManifestEngine.DefaultEngagementManifestEngine(flags));
    }
}
