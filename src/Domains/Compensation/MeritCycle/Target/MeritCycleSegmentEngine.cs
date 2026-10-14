namespace Acme.Workforce.Compensation.MeritCycle.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle segment calculation.</summary>
public interface IMeritCycleSegmentAdjuster
{
    string Describe();
    double Adjust(MeritCycleSegment item, double valueValue);
}

public sealed record PercentMeritCycleSegmentAdjuster(double Percent) : IMeritCycleSegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MeritCycleSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingMeritCycleSegmentAdjuster(double Step) : IMeritCycleSegmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MeritCycleSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredMeritCycleSegmentAdjuster(List<double> Bounds, List<double> Rates) : IMeritCycleSegmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MeritCycleSegment item, double valueValue)
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

public sealed record MeritCycleSegmentRatingOffsetAdjuster(double Weight) : IMeritCycleSegmentAdjuster
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(MeritCycleSegment item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record TitleMeritCycleSegmentAdjuster(Dictionary<string, double> Factors) : IMeritCycleSegmentAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(MeritCycleSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the merit cycle segment steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleSegmentEngine(List<IMeritCycleSegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleSegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleSegmentEngine DefaultMeritCycleSegmentEngine()
    {
        return new MeritCycleSegmentEngine(
            new List<IMeritCycleSegmentAdjuster>
            {
                new PercentMeritCycleSegmentAdjuster(11.3),
                new RoundingMeritCycleSegmentAdjuster(1.0),
                new TieredMeritCycleSegmentAdjuster(
                    new List<double> { 2308.0, 3873.0, 4499.0, 5487.0 },
                    new List<double> { 1.18, 1.15, 0.93, 1.19 }),
                new MeritCycleSegmentRatingOffsetAdjuster(1.15),
                new TitleMeritCycleSegmentAdjuster(new Dictionary<string, double> { ["central"] = 0.96 }),
            },
            new List<string>(),
            4.4,
            42575.5);
    }

    public double Compute(MeritCycleSegment item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
