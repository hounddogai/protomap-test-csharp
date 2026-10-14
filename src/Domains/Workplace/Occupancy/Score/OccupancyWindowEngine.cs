namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the occupancy window calculation.</summary>
public interface IOccupancyWindowAdjuster
{
    string Describe();
    double Adjust(OccupancyWindow item, double valueValue);
}

public sealed record PercentOccupancyWindowAdjuster(double Percent) : IOccupancyWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OccupancyWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingOccupancyWindowAdjuster(double Step) : IOccupancyWindowAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OccupancyWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelOccupancyWindowAdjuster(Dictionary<string, double> Factors) : IOccupancyWindowAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(OccupancyWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record OccupancyWindowDiscountOffsetAdjuster(double Weight) : IOccupancyWindowAdjuster
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(OccupancyWindow item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record MinimumOccupancyWindowAdjuster(double Minimum) : IOccupancyWindowAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OccupancyWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedOccupancyWindowAdjuster(Registry Flags, string Flag, double Percent) : IOccupancyWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancyWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the occupancy window steps in order and keeps a trace of each.</summary>
public sealed class OccupancyWindowEngine(List<IOccupancyWindowAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyWindowAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyWindowEngine DefaultOccupancyWindowEngine(Registry flags)
    {
        return new OccupancyWindowEngine(
            new List<IOccupancyWindowAdjuster>
            {
                new PercentOccupancyWindowAdjuster(-20.8),
                new RoundingOccupancyWindowAdjuster(5.0),
                new LabelOccupancyWindowAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.96, ["central"] = 1.06 }),
                new OccupancyWindowDiscountOffsetAdjuster(0.55),
                new MinimumOccupancyWindowAdjuster(47.7),
                new FlaggedOccupancyWindowAdjuster(flags, "occupancy-window-scheduled", -3.4),
            },
            new List<string>(),
            1.7,
            24652.5);
    }

    public double Run(OccupancyWindow item)
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
