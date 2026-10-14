namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday allocation calculation.</summary>
public interface IHolidayAllocationStep
{
    string Describe();
    double Adjust(HolidayAllocation item, double valueValue);
}

public sealed record HolidayAllocationScoreOffsetAdjuster(double Weight) : IHolidayAllocationStep
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(HolidayAllocation item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record PercentHolidayAllocationAdjuster(double Percent) : IHolidayAllocationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingHolidayAllocationAdjuster(double Step) : IHolidayAllocationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidayAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameHolidayAllocationAdjuster(Dictionary<string, double> Factors) : IHolidayAllocationStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(HolidayAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the holiday allocation steps in order and keeps a trace of each.</summary>
public sealed class HolidayAllocationEngine(List<IHolidayAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayAllocationEngine DefaultHolidayAllocationEngine()
    {
        return new HolidayAllocationEngine(
            new List<IHolidayAllocationStep>
            {
                new HolidayAllocationScoreOffsetAdjuster(1.79),
                new PercentHolidayAllocationAdjuster(16.7),
                new RoundingHolidayAllocationAdjuster(0.5),
                new NameHolidayAllocationAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.29, ["north"] = 1.23, ["retail"] = 1.08 }),
            },
            new List<string>(),
            0.8,
            21298.4);
    }

    public double Run(HolidayAllocation item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
