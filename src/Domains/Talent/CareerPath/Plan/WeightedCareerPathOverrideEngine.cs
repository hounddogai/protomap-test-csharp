namespace Acme.Workforce.Talent.CareerPath.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted career path override calculation.</summary>
public interface IWeightedCareerPathOverrideModifier
{
    string Describe();
    double Adjust(WeightedCareerPathOverride item, double valueValue);
}

public sealed record WeightedCareerPathOverrideHoursOffsetAdjuster(double Weight) : IWeightedCareerPathOverrideModifier
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(WeightedCareerPathOverride item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record FlaggedWeightedCareerPathOverrideAdjuster(Registry Flags, string Flag, double Percent) : IWeightedCareerPathOverrideModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedCareerPathOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumWeightedCareerPathOverrideAdjuster(double Minimum) : IWeightedCareerPathOverrideModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedCareerPathOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapWeightedCareerPathOverrideAdjuster(double Cap) : IWeightedCareerPathOverrideModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedCareerPathOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentWeightedCareerPathOverrideAdjuster(double Percent) : IWeightedCareerPathOverrideModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedCareerPathOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentWeightedCareerPathOverrideAdjuster(Dictionary<string, double> Factors) : IWeightedCareerPathOverrideModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(WeightedCareerPathOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the weighted career path override steps in order and keeps a trace of each.</summary>
public sealed class WeightedCareerPathOverrideEngine(List<IWeightedCareerPathOverrideModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedCareerPathOverrideModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedCareerPathOverrideEngine DefaultWeightedCareerPathOverrideEngine(Registry flags)
    {
        return new WeightedCareerPathOverrideEngine(
            new List<IWeightedCareerPathOverrideModifier>
            {
                new WeightedCareerPathOverrideHoursOffsetAdjuster(1.87),
                new FlaggedWeightedCareerPathOverrideAdjuster(flags, "weighted-career-path-override-batched", -0.4),
                new MinimumWeightedCareerPathOverrideAdjuster(47.5),
                new CapWeightedCareerPathOverrideAdjuster(33742.0),
                new PercentWeightedCareerPathOverrideAdjuster(-22.5),
                new SegmentWeightedCareerPathOverrideAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.01, ["south"] = 1.19 }),
            },
            new List<string>(),
            1.5,
            76806.1);
    }

    public double Evaluate(WeightedCareerPathOverride item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedCareerPathOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
