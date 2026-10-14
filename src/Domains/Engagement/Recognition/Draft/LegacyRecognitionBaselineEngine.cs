namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy recognition baseline calculation.</summary>
public interface ILegacyRecognitionBaselineFactor
{
    string Describe();
    double Adjust(LegacyRecognitionBaseline item, double valueValue);
}

public sealed record RoundingLegacyRecognitionBaselineAdjuster(double Step) : ILegacyRecognitionBaselineFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyRecognitionBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LegacyRecognitionBaselineTenureMonthsOffsetAdjuster(double Weight) : ILegacyRecognitionBaselineFactor
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(LegacyRecognitionBaseline item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record FlaggedLegacyRecognitionBaselineAdjuster(Registry Flags, string Flag, double Percent) : ILegacyRecognitionBaselineFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyRecognitionBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamLegacyRecognitionBaselineAdjuster(Dictionary<string, double> Factors) : ILegacyRecognitionBaselineFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(LegacyRecognitionBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the legacy recognition baseline steps in order and keeps a trace of each.</summary>
public sealed class LegacyRecognitionBaselineEngine(List<ILegacyRecognitionBaselineFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyRecognitionBaselineFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyRecognitionBaselineEngine DefaultLegacyRecognitionBaselineEngine(Registry flags)
    {
        return new LegacyRecognitionBaselineEngine(
            new List<ILegacyRecognitionBaselineFactor>
            {
                new RoundingLegacyRecognitionBaselineAdjuster(0.1),
                new LegacyRecognitionBaselineTenureMonthsOffsetAdjuster(1.04),
                new FlaggedLegacyRecognitionBaselineAdjuster(flags, "legacy-recognition-baseline-scheduled", 2.8),
                new TeamLegacyRecognitionBaselineAdjuster(new Dictionary<string, double> { ["north"] = 1.11 }),
            },
            new List<string>(),
            2.9,
            6746.9);
    }

    public double Run(LegacyRecognitionBaseline item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyRecognitionBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
