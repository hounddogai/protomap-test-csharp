namespace Acme.Workforce.Compensation.MeritCycle.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped merit cycle scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedMeritCycleScoreDesk(ICappedMeritCycleScoreStore Store, CappedMeritCycleScoreValidator Validator, IClock Clock, List<string> Journal, CappedMeritCycleScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped merit cycle score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedMeritCycleScoreStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped merit cycle score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedMeritCycleScoreStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped merit cycle score {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of capped merit cycle score workflow.</summary>
public static class CappedMeritCycleScoreWorkflow
{
    public static CappedMeritCycleScoreDesk NewCappedMeritCycleScoreDesk(ICappedMeritCycleScoreStore store, IClock clock, Registry flags)
    {
        return new CappedMeritCycleScoreDesk(
            store,
            CappedMeritCycleScoreRules.DefaultCappedMeritCycleScoreValidator(),
            clock,
            new List<string>(),
            CappedMeritCycleScoreEngine.DefaultCappedMeritCycleScoreEngine(flags));
    }
}
