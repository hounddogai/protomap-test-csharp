namespace Acme.Workforce.Engagement.Recognition.Segment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the default recognition policy calculation.</summary>
public interface IDefaultRecognitionPolicyFactor
{
    string Describe();
    double Adjust(DefaultRecognitionPolicy item, double valueValue);
}

public sealed record PercentDefaultRecognitionPolicyAdjuster(double Percent) : IDefaultRecognitionPolicyFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultRecognitionPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentDefaultRecognitionPolicyAdjuster(Dictionary<string, double> Factors) : IDefaultRecognitionPolicyFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(DefaultRecognitionPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapDefaultRecognitionPolicyAdjuster(double Cap) : IDefaultRecognitionPolicyFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultRecognitionPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedDefaultRecognitionPolicyAdjuster(Registry Flags, string Flag, double Percent) : IDefaultRecognitionPolicyFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultRecognitionPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDefaultRecognitionPolicyAdjuster(double Minimum) : IDefaultRecognitionPolicyFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultRecognitionPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default recognition policy steps in order and keeps a trace of each.</summary>
public sealed class DefaultRecognitionPolicyEngine(List<IDefaultRecognitionPolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultRecognitionPolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultRecognitionPolicyEngine DefaultDefaultRecognitionPolicyEngine(Registry flags)
    {
        return new DefaultRecognitionPolicyEngine(
            new List<IDefaultRecognitionPolicyFactor>
            {
                new PercentDefaultRecognitionPolicyAdjuster(-11.0),
                new SegmentDefaultRecognitionPolicyAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.9, ["online"] = 1.12, ["west"] = 0.84 }),
                new CapDefaultRecognitionPolicyAdjuster(7296.0),
                new FlaggedDefaultRecognitionPolicyAdjuster(flags, "default-recognition-policy-rolling", -6.5),
                new MinimumDefaultRecognitionPolicyAdjuster(28.6),
            },
            new List<string>(),
            1.6,
            48497.9);
    }

    public double Evaluate(DefaultRecognitionPolicy item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultRecognitionPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
