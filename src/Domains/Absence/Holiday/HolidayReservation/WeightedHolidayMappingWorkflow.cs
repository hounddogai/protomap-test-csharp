namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted holiday mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedHolidayMappingCoordinator(IWeightedHolidayMappingStore Store, WeightedHolidayMappingValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted holiday mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedHolidayMappingStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted holiday mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedHolidayMappingStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of weighted holiday mapping workflow.</summary>
public static class WeightedHolidayMappingWorkflow
{
    public static WeightedHolidayMappingCoordinator NewWeightedHolidayMappingCoordinator(IWeightedHolidayMappingStore store, IClock clock)
    {
        return new WeightedHolidayMappingCoordinator(
            store,
            WeightedHolidayMappingRules.DefaultWeightedHolidayMappingValidator(),
            clock,
            new List<string>());
    }
}
