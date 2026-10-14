namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal holiday adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalHolidayAdjustmentCoordinator(ISeasonalHolidayAdjustmentStore Store, SeasonalHolidayAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal holiday adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalHolidayAdjustmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal holiday adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalHolidayAdjustmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of seasonal holiday adjustment workflow.</summary>
public static class SeasonalHolidayAdjustmentWorkflow
{
    public static SeasonalHolidayAdjustmentCoordinator NewSeasonalHolidayAdjustmentCoordinator(ISeasonalHolidayAdjustmentStore store, IClock clock)
    {
        return new SeasonalHolidayAdjustmentCoordinator(
            store,
            SeasonalHolidayAdjustmentRules.DefaultSeasonalHolidayAdjustmentValidator(),
            clock,
            new List<string>());
    }
}
