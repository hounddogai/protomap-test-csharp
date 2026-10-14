namespace Acme.Workforce.Compensation.MeritCycle.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict merit cycle plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictMeritCyclePlanCoordinator(IStrictMeritCyclePlanStore Store, StrictMeritCyclePlanValidator Validator, IClock Clock, List<string> Journal, StrictMeritCyclePlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict merit cycle plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictMeritCyclePlanStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict merit cycle plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictMeritCyclePlanStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict merit cycle plan {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of strict merit cycle plan workflow.</summary>
public static class StrictMeritCyclePlanWorkflow
{
    public static StrictMeritCyclePlanCoordinator NewStrictMeritCyclePlanCoordinator(IStrictMeritCyclePlanStore store, IClock clock)
    {
        return new StrictMeritCyclePlanCoordinator(
            store,
            StrictMeritCyclePlanRules.DefaultStrictMeritCyclePlanValidator(),
            clock,
            new List<string>(),
            StrictMeritCyclePlanEngine.DefaultStrictMeritCyclePlanEngine());
    }
}
