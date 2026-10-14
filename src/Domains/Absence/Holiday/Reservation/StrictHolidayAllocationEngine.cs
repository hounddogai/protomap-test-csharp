namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict holiday allocation calculation.</summary>
public interface IStrictHolidayAllocationStep
{
    string Describe();
    double Adjust(StrictHolidayAllocation item, double valueValue);
}

public sealed record StrictHolidayAllocationUpdatedAtOffsetAdjuster(double Weight) : IStrictHolidayAllocationStep
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(StrictHolidayAllocation item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record MinimumStrictHolidayAllocationAdjuster(double Minimum) : IStrictHolidayAllocationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictHolidayAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredStrictHolidayAllocationAdjuster(List<double> Bounds, List<double> Rates) : IStrictHolidayAllocationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictHolidayAllocation item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record RoundingStrictHolidayAllocationAdjuster(double Step) : IStrictHolidayAllocationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictHolidayAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the strict holiday allocation steps in order and keeps a trace of each.</summary>
public sealed class StrictHolidayAllocationEngine(List<IStrictHolidayAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictHolidayAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictHolidayAllocationEngine DefaultStrictHolidayAllocationEngine()
    {
        return new StrictHolidayAllocationEngine(
            new List<IStrictHolidayAllocationStep>
            {
                new StrictHolidayAllocationUpdatedAtOffsetAdjuster(0.26),
                new MinimumStrictHolidayAllocationAdjuster(36.1),
                new TieredStrictHolidayAllocationAdjuster(
                    new List<double> { 8127.0, 8545.0 },
                    new List<double> { 1.06, 1.07 }),
                new RoundingStrictHolidayAllocationAdjuster(0.1),
            },
            new List<string>(),
            4.1,
            6067.0);
    }

    public double Run(StrictHolidayAllocation item)
    {
        var current = item.EffectiveTotal();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<StrictHolidayAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
