namespace Acme.Workforce.Absence.Holiday.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday window calculation.</summary>
public interface IHolidayWindowAdjuster
{
    string Describe();
    double Adjust(HolidayWindow item, double valueValue);
}

public sealed record RoundingHolidayWindowAdjuster(double Step) : IHolidayWindowAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidayWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentHolidayWindowAdjuster(double Percent) : IHolidayWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record HolidayWindowRateOffsetAdjuster(double Weight) : IHolidayWindowAdjuster
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(HolidayWindow item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the holiday window steps in order and keeps a trace of each.</summary>
public sealed class HolidayWindowEngine(List<IHolidayWindowAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayWindowAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayWindowEngine DefaultHolidayWindowEngine()
    {
        return new HolidayWindowEngine(
            new List<IHolidayWindowAdjuster>
            {
                new RoundingHolidayWindowAdjuster(0.5),
                new PercentHolidayWindowAdjuster(22.0),
                new HolidayWindowRateOffsetAdjuster(0.81),
            },
            new List<string>(),
            4.7,
            30696.0);
    }

    public double Compute(HolidayWindow item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
