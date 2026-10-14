namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday profile calculation.</summary>
public interface IHolidayProfileAdjuster
{
    string Describe();
    double Adjust(HolidayProfile item, double valueValue);
}

public sealed record MinimumHolidayProfileAdjuster(double Minimum) : IHolidayProfileAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HolidayProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SourceHolidayProfileAdjuster(Dictionary<string, double> Factors) : IHolidayProfileAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(HolidayProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingHolidayProfileAdjuster(double Step) : IHolidayProfileAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidayProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentHolidayProfileAdjuster(double Percent) : IHolidayProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the holiday profile steps in order and keeps a trace of each.</summary>
public sealed class HolidayProfileEngine(List<IHolidayProfileAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayProfileAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayProfileEngine DefaultHolidayProfileEngine()
    {
        return new HolidayProfileEngine(
            new List<IHolidayProfileAdjuster>
            {
                new MinimumHolidayProfileAdjuster(35.9),
                new SourceHolidayProfileAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.87, ["retail"] = 1.07 }),
                new RoundingHolidayProfileAdjuster(0.25),
                new PercentHolidayProfileAdjuster(-4.9),
            },
            new List<string>(),
            1.7,
            96435.6);
    }

    public double Compute(HolidayProfile item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
