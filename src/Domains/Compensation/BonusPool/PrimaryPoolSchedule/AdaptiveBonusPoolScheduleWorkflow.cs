namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive bonus pool schedules through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveBonusPoolScheduleManager(IAdaptiveBonusPoolScheduleStore Store, AdaptiveBonusPoolScheduleValidator Validator, IClock Clock, List<string> Journal, AdaptiveBonusPoolScheduleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive bonus pool schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveBonusPoolScheduleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive bonus pool schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveBonusPoolScheduleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive bonus pool schedule {code} not found");
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

/// <summary>Functions of adaptive bonus pool schedule workflow.</summary>
public static class AdaptiveBonusPoolScheduleWorkflow
{
    public static AdaptiveBonusPoolScheduleManager NewAdaptiveBonusPoolScheduleManager(IAdaptiveBonusPoolScheduleStore store, IClock clock, Registry flags)
    {
        return new AdaptiveBonusPoolScheduleManager(
            store,
            AdaptiveBonusPoolScheduleRules.DefaultAdaptiveBonusPoolScheduleValidator(),
            clock,
            new List<string>(),
            AdaptiveBonusPoolScheduleEngine.DefaultAdaptiveBonusPoolScheduleEngine(flags));
    }
}
