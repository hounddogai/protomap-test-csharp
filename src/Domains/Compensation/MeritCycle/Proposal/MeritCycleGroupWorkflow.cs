namespace Acme.Workforce.Compensation.MeritCycle.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves merit cycle groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleGroupCoordinator(IMeritCycleGroupStore Store, MeritCycleGroupValidator Validator, IClock Clock, List<string> Journal, MeritCycleGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleGroupStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleGroupStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle group {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of merit cycle group workflow.</summary>
public static class MeritCycleGroupWorkflow
{
    public static MeritCycleGroupCoordinator NewMeritCycleGroupCoordinator(IMeritCycleGroupStore store, IClock clock, Registry flags)
    {
        return new MeritCycleGroupCoordinator(
            store,
            MeritCycleGroupRules.DefaultMeritCycleGroupValidator(),
            clock,
            new List<string>(),
            MeritCycleGroupEngine.DefaultMeritCycleGroupEngine(flags));
    }
}
