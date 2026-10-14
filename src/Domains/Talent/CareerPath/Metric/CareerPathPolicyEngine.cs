namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path policy calculation.</summary>
public interface ICareerPathPolicyFactor
{
    string Describe();
    double Adjust(CareerPathPolicy item, double valueValue);
}

public sealed record FlaggedCareerPathPolicyAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathPolicyFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentCareerPathPolicyAdjuster(double Percent) : ICareerPathPolicyFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CareerPathPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CareerPathPolicyEndsAtOffsetAdjuster(double Weight) : ICareerPathPolicyFactor
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(CareerPathPolicy item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record SegmentCareerPathPolicyAdjuster(Dictionary<string, double> Factors) : ICareerPathPolicyFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(CareerPathPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the career path policy steps in order and keeps a trace of each.</summary>
public sealed class CareerPathPolicyEngine(List<ICareerPathPolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathPolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathPolicyEngine DefaultCareerPathPolicyEngine(Registry flags)
    {
        return new CareerPathPolicyEngine(
            new List<ICareerPathPolicyFactor>
            {
                new FlaggedCareerPathPolicyAdjuster(flags, "career-path-policy-regional", 8.4),
                new PercentCareerPathPolicyAdjuster(12.7),
                new CareerPathPolicyEndsAtOffsetAdjuster(1.24),
                new SegmentCareerPathPolicyAdjuster(new Dictionary<string, double> { ["west"] = 0.75 }),
            },
            new List<string>(),
            3.3,
            7407.5);
    }

    public double Compute(CareerPathPolicy item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
