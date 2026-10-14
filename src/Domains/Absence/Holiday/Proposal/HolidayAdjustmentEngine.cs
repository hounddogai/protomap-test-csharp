namespace Acme.Workforce.Absence.Holiday.Proposal;

/// <summary>One step of the holiday adjustment calculation.</summary>
public interface IHolidayAdjustmentFactor
{
    string Describe();
    double Adjust(HolidayAdjustment item, double valueValue);
}

public sealed record MinimumHolidayAdjustmentAdjuster(double Minimum) : IHolidayAdjustmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HolidayAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentHolidayAdjustmentAdjuster(double Percent) : IHolidayAdjustmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record HolidayAdjustmentRatioOffsetAdjuster(double Weight) : IHolidayAdjustmentFactor
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(HolidayAdjustment item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record CapHolidayAdjustmentAdjuster(double Cap) : IHolidayAdjustmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HolidayAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the holiday adjustment steps in order and keeps a trace of each.</summary>
public sealed class HolidayAdjustmentEngine(List<IHolidayAdjustmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayAdjustmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayAdjustmentEngine DefaultHolidayAdjustmentEngine()
    {
        return new HolidayAdjustmentEngine(
            new List<IHolidayAdjustmentFactor>
            {
                new MinimumHolidayAdjustmentAdjuster(22.0),
                new PercentHolidayAdjustmentAdjuster(9.7),
                new HolidayAdjustmentRatioOffsetAdjuster(1.25),
                new CapHolidayAdjustmentAdjuster(35187.0),
            },
            new List<string>(),
            4.0,
            12699.9);
    }

    public double Price(HolidayAdjustment item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
