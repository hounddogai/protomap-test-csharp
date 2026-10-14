namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves merit cycle scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleScoreCoordinator(IMeritCycleScoreStore Store, MeritCycleScoreValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleScoreStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleScoreStatus.Expired);
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

/// <summary>Functions of merit cycle score workflow.</summary>
public static class MeritCycleScoreWorkflow
{
    public static MeritCycleScoreCoordinator NewMeritCycleScoreCoordinator(IMeritCycleScoreStore store, IClock clock)
    {
        return new MeritCycleScoreCoordinator(
            store,
            MeritCycleScoreRules.DefaultMeritCycleScoreValidator(),
            clock,
            new List<string>());
    }
}
