namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal bonus pool schedules through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalBonusPoolScheduleService(ISeasonalBonusPoolScheduleStore Store, SeasonalBonusPoolScheduleValidator Validator, IClock Clock, List<string> Journal, SeasonalBonusPoolScheduleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal bonus pool schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalBonusPoolScheduleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal bonus pool schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalBonusPoolScheduleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal bonus pool schedule {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of seasonal bonus pool schedule workflow.</summary>
public static class SeasonalBonusPoolScheduleWorkflow
{
    public static SeasonalBonusPoolScheduleService NewSeasonalBonusPoolScheduleService(ISeasonalBonusPoolScheduleStore store, IClock clock)
    {
        return new SeasonalBonusPoolScheduleService(
            store,
            SeasonalBonusPoolScheduleRules.DefaultSeasonalBonusPoolScheduleValidator(),
            clock,
            new List<string>(),
            SeasonalBonusPoolScheduleEngine.DefaultSeasonalBonusPoolScheduleEngine());
    }
}
