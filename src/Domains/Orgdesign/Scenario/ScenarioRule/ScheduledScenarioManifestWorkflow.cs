namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled scenario manifests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledScenarioManifestDesk(IScheduledScenarioManifestStore Store, ScheduledScenarioManifestValidator Validator, IClock Clock, List<string> Journal, ScheduledScenarioManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled scenario manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledScenarioManifestStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled scenario manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledScenarioManifestStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled scenario manifest {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of scheduled scenario manifest workflow.</summary>
public static class ScheduledScenarioManifestWorkflow
{
    public static ScheduledScenarioManifestDesk NewScheduledScenarioManifestDesk(IScheduledScenarioManifestStore store, IClock clock, Registry flags)
    {
        return new ScheduledScenarioManifestDesk(
            store,
            ScheduledScenarioManifestRules.DefaultScheduledScenarioManifestValidator(),
            clock,
            new List<string>(),
            ScheduledScenarioManifestEngine.DefaultScheduledScenarioManifestEngine(flags));
    }
}
