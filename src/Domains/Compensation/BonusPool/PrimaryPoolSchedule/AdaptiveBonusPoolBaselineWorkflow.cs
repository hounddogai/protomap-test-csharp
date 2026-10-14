namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive bonus pool baselines through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveBonusPoolBaselineCoordinator(IAdaptiveBonusPoolBaselineStore Store, AdaptiveBonusPoolBaselineValidator Validator, IClock Clock, List<string> Journal, AdaptiveBonusPoolBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive bonus pool baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveBonusPoolBaselineStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive bonus pool baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveBonusPoolBaselineStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive bonus pool baseline {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of adaptive bonus pool baseline workflow.</summary>
public static class AdaptiveBonusPoolBaselineWorkflow
{
    public static AdaptiveBonusPoolBaselineCoordinator NewAdaptiveBonusPoolBaselineCoordinator(IAdaptiveBonusPoolBaselineStore store, IClock clock)
    {
        return new AdaptiveBonusPoolBaselineCoordinator(
            store,
            AdaptiveBonusPoolBaselineRules.DefaultAdaptiveBonusPoolBaselineValidator(),
            clock,
            new List<string>(),
            AdaptiveBonusPoolBaselineEngine.DefaultAdaptiveBonusPoolBaselineEngine());
    }
}
