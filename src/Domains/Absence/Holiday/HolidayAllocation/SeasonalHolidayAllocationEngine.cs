namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal holiday allocation calculation.</summary>
public interface ISeasonalHolidayAllocationModifier
{
    string Describe();
    double Adjust(SeasonalHolidayAllocation item, double valueValue);
}

public sealed record MinimumSeasonalHolidayAllocationAdjuster(double Minimum) : ISeasonalHolidayAllocationModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalHolidayAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapSeasonalHolidayAllocationAdjuster(double Cap) : ISeasonalHolidayAllocationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalHolidayAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredSeasonalHolidayAllocationAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalHolidayAllocationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalHolidayAllocation item, double valueValue)
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

public sealed record PercentSeasonalHolidayAllocationAdjuster(double Percent) : ISeasonalHolidayAllocationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalHolidayAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal holiday allocation steps in order and keeps a trace of each.</summary>
public sealed class SeasonalHolidayAllocationEngine(List<ISeasonalHolidayAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalHolidayAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalHolidayAllocationEngine DefaultSeasonalHolidayAllocationEngine()
    {
        return new SeasonalHolidayAllocationEngine(
            new List<ISeasonalHolidayAllocationModifier>
            {
                new MinimumSeasonalHolidayAllocationAdjuster(14.7),
                new CapSeasonalHolidayAllocationAdjuster(13024.0),
                new TieredSeasonalHolidayAllocationAdjuster(
                    new List<double> { 2936.0, 5833.0, 6424.0, 7620.0 },
                    new List<double> { 0.96, 0.85, 0.93, 0.81 }),
                new PercentSeasonalHolidayAllocationAdjuster(-13.0),
            },
            new List<string>(),
            4.2,
            48577.9);
    }

    public double Price(SeasonalHolidayAllocation item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<SeasonalHolidayAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
