namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves weighted merit cycle bundles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedMeritCycleBundleCoordinator(IWeightedMeritCycleBundleStore Store, WeightedMeritCycleBundleValidator Validator, IClock Clock, List<string> Journal, WeightedMeritCycleBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted merit cycle bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedMeritCycleBundleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted merit cycle bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedMeritCycleBundleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted merit cycle bundle {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of weighted merit cycle bundle workflow.</summary>
public static class WeightedMeritCycleBundleWorkflow
{
    public static WeightedMeritCycleBundleCoordinator NewWeightedMeritCycleBundleCoordinator(IWeightedMeritCycleBundleStore store, IClock clock, Registry flags)
    {
        return new WeightedMeritCycleBundleCoordinator(
            store,
            WeightedMeritCycleBundleRules.DefaultWeightedMeritCycleBundleValidator(),
            clock,
            new List<string>(),
            WeightedMeritCycleBundleEngine.DefaultWeightedMeritCycleBundleEngine(flags));
    }
}
