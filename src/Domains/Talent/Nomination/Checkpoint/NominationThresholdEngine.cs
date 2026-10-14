namespace Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>One step of the nomination threshold calculation.</summary>
public interface INominationThresholdAdjuster
{
    string Describe();
    double Adjust(NominationThreshold item, double valueValue);
}

public sealed record MinimumNominationThresholdAdjuster(double Minimum) : INominationThresholdAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(NominationThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapNominationThresholdAdjuster(double Cap) : INominationThresholdAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NominationThresholdEndsAtOffsetAdjuster(double Weight) : INominationThresholdAdjuster
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(NominationThreshold item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the nomination threshold steps in order and keeps a trace of each.</summary>
public sealed class NominationThresholdEngine(List<INominationThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationThresholdEngine DefaultNominationThresholdEngine()
    {
        return new NominationThresholdEngine(
            new List<INominationThresholdAdjuster>
            {
                new MinimumNominationThresholdAdjuster(39.5),
                new CapNominationThresholdAdjuster(28967.0),
                new NominationThresholdEndsAtOffsetAdjuster(0.31),
            },
            new List<string>(),
            1.1,
            68753.1);
    }

    public double Compute(NominationThreshold item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<NominationThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
