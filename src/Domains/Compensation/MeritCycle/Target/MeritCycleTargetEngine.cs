namespace Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>One step of the merit cycle target calculation.</summary>
public interface IMeritCycleTargetModifier
{
    string Describe();
    double Adjust(MeritCycleTarget item, double valueValue);
}

public sealed record MinimumMeritCycleTargetAdjuster(double Minimum) : IMeritCycleTargetModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MeritCycleTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record MeritCycleTargetBatchSizeOffsetAdjuster(double Weight) : IMeritCycleTargetModifier
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(MeritCycleTarget item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record SegmentMeritCycleTargetAdjuster(Dictionary<string, double> Factors) : IMeritCycleTargetModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(MeritCycleTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the merit cycle target steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleTargetEngine(List<IMeritCycleTargetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleTargetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleTargetEngine DefaultMeritCycleTargetEngine()
    {
        return new MeritCycleTargetEngine(
            new List<IMeritCycleTargetModifier>
            {
                new MinimumMeritCycleTargetAdjuster(4.7),
                new MeritCycleTargetBatchSizeOffsetAdjuster(1.45),
                new SegmentMeritCycleTargetAdjuster(new Dictionary<string, double> { ["retail"] = 1.03 }),
            },
            new List<string>(),
            0.8,
            45751.3);
    }

    public double Price(MeritCycleTarget item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
