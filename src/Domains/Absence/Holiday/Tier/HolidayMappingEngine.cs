namespace Acme.Workforce.Absence.Holiday.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday mapping calculation.</summary>
public interface IHolidayMappingStep
{
    string Describe();
    double Adjust(HolidayMapping item, double valueValue);
}

public sealed record PercentHolidayMappingAdjuster(double Percent) : IHolidayMappingStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapHolidayMappingAdjuster(double Cap) : IHolidayMappingStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HolidayMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumHolidayMappingAdjuster(double Minimum) : IHolidayMappingStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HolidayMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DepartmentHolidayMappingAdjuster(Dictionary<string, double> Factors) : IHolidayMappingStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(HolidayMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredHolidayMappingAdjuster(List<double> Bounds, List<double> Rates) : IHolidayMappingStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HolidayMapping item, double valueValue)
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

/// <summary>Runs the holiday mapping steps in order and keeps a trace of each.</summary>
public sealed class HolidayMappingEngine(List<IHolidayMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayMappingEngine DefaultHolidayMappingEngine()
    {
        return new HolidayMappingEngine(
            new List<IHolidayMappingStep>
            {
                new PercentHolidayMappingAdjuster(-7.6),
                new CapHolidayMappingAdjuster(47805.0),
                new MinimumHolidayMappingAdjuster(21.5),
                new DepartmentHolidayMappingAdjuster(new Dictionary<string, double> { ["south"] = 1.11 }),
                new TieredHolidayMappingAdjuster(
                    new List<double> { 2896.0, 3416.0, 4365.0 },
                    new List<double> { 1.09, 0.96, 1.15 }),
            },
            new List<string>(),
            0.1,
            70545.1);
    }

    public double Price(HolidayMapping item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
