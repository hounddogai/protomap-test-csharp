namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday group calculation.</summary>
public interface IHolidayGroupFactor
{
    string Describe();
    double Adjust(HolidayGroup item, double valueValue);
}

public sealed record HolidayGroupQuantityOffsetAdjuster(double Weight) : IHolidayGroupFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(HolidayGroup item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record PercentHolidayGroupAdjuster(double Percent) : IHolidayGroupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumHolidayGroupAdjuster(double Minimum) : IHolidayGroupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HolidayGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the holiday group steps in order and keeps a trace of each.</summary>
public sealed class HolidayGroupEngine(List<IHolidayGroupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayGroupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayGroupEngine DefaultHolidayGroupEngine()
    {
        return new HolidayGroupEngine(
            new List<IHolidayGroupFactor>
            {
                new HolidayGroupQuantityOffsetAdjuster(0.54),
                new PercentHolidayGroupAdjuster(0.2),
                new MinimumHolidayGroupAdjuster(5.4),
            },
            new List<string>(),
            1.4,
            5199.7);
    }

    public double Evaluate(HolidayGroup item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
