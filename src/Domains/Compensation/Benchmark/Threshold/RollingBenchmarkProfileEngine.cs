namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rolling benchmark profile calculation.</summary>
public interface IRollingBenchmarkProfileModifier
{
    string Describe();
    double Adjust(RollingBenchmarkProfile item, double valueValue);
}

public sealed record FlaggedRollingBenchmarkProfileAdjuster(Registry Flags, string Flag, double Percent) : IRollingBenchmarkProfileModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingBenchmarkProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryRollingBenchmarkProfileAdjuster(Dictionary<string, double> Factors) : IRollingBenchmarkProfileModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(RollingBenchmarkProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RollingBenchmarkProfilePriorityOffsetAdjuster(double Weight) : IRollingBenchmarkProfileModifier
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(RollingBenchmarkProfile item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record PercentRollingBenchmarkProfileAdjuster(double Percent) : IRollingBenchmarkProfileModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingBenchmarkProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRollingBenchmarkProfileAdjuster(double Minimum) : IRollingBenchmarkProfileModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingBenchmarkProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRollingBenchmarkProfileAdjuster(double Cap) : IRollingBenchmarkProfileModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingBenchmarkProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling benchmark profile steps in order and keeps a trace of each.</summary>
public sealed class RollingBenchmarkProfileEngine(List<IRollingBenchmarkProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingBenchmarkProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingBenchmarkProfileEngine DefaultRollingBenchmarkProfileEngine(Registry flags)
    {
        return new RollingBenchmarkProfileEngine(
            new List<IRollingBenchmarkProfileModifier>
            {
                new FlaggedRollingBenchmarkProfileAdjuster(flags, "rolling-benchmark-profile-regional", 0.8),
                new CountryRollingBenchmarkProfileAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.76, ["south"] = 0.99, ["east"] = 1.35 }),
                new RollingBenchmarkProfilePriorityOffsetAdjuster(1.17),
                new PercentRollingBenchmarkProfileAdjuster(15.6),
                new MinimumRollingBenchmarkProfileAdjuster(28.8),
                new CapRollingBenchmarkProfileAdjuster(40425.0),
            },
            new List<string>(),
            2.0,
            77422.9);
    }

    public double Price(RollingBenchmarkProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<RollingBenchmarkProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
