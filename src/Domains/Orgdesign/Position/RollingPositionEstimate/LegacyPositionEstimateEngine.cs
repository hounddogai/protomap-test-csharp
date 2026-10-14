namespace Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy position estimate calculation.</summary>
public interface ILegacyPositionEstimateFactor
{
    string Describe();
    double Adjust(LegacyPositionEstimate item, double valueValue);
}

public sealed record RoundingLegacyPositionEstimateAdjuster(double Step) : ILegacyPositionEstimateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyPositionEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLegacyPositionEstimateAdjuster(Registry Flags, string Flag, double Percent) : ILegacyPositionEstimateFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyPositionEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelLegacyPositionEstimateAdjuster(Dictionary<string, double> Factors) : ILegacyPositionEstimateFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LegacyPositionEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LegacyPositionEstimateDiscountOffsetAdjuster(double Weight) : ILegacyPositionEstimateFactor
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(LegacyPositionEstimate item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record CapLegacyPositionEstimateAdjuster(double Cap) : ILegacyPositionEstimateFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyPositionEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the legacy position estimate steps in order and keeps a trace of each.</summary>
public sealed class LegacyPositionEstimateEngine(List<ILegacyPositionEstimateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPositionEstimateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPositionEstimateEngine DefaultLegacyPositionEstimateEngine(Registry flags)
    {
        return new LegacyPositionEstimateEngine(
            new List<ILegacyPositionEstimateFactor>
            {
                new RoundingLegacyPositionEstimateAdjuster(0.1),
                new FlaggedLegacyPositionEstimateAdjuster(flags, "legacy-position-estimate-weighted", 1.1),
                new LabelLegacyPositionEstimateAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.73, ["retail"] = 0.97 }),
                new LegacyPositionEstimateDiscountOffsetAdjuster(0.89),
                new CapLegacyPositionEstimateAdjuster(20580.0),
            },
            new List<string>(),
            4.1,
            59984.3);
    }

    public double Compute(LegacyPositionEstimate item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPositionEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
