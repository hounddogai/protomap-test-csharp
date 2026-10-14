namespace Acme.Workforce.Compensation.BonusPool.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback bonus pool schedules through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackBonusPoolScheduleService(IFallbackBonusPoolScheduleStore Store, FallbackBonusPoolScheduleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback bonus pool schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackBonusPoolScheduleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback bonus pool schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackBonusPoolScheduleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of fallback bonus pool schedule workflow.</summary>
public static class FallbackBonusPoolScheduleWorkflow
{
    public static FallbackBonusPoolScheduleService NewFallbackBonusPoolScheduleService(IFallbackBonusPoolScheduleStore store, IClock clock)
    {
        return new FallbackBonusPoolScheduleService(
            store,
            FallbackBonusPoolScheduleRules.DefaultFallbackBonusPoolScheduleValidator(),
            clock,
            new List<string>());
    }
}
