namespace Acme.Workforce.Compensation.Benchmark.Preference;

/// <summary>One step of the rolling benchmark proposal calculation.</summary>
public interface IRollingBenchmarkProposalStep
{
    string Describe();
    double Adjust(RollingBenchmarkProposal item, double valueValue);
}

public sealed record PercentRollingBenchmarkProposalAdjuster(double Percent) : IRollingBenchmarkProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingBenchmarkProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRollingBenchmarkProposalAdjuster(double Minimum) : IRollingBenchmarkProposalStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingBenchmarkProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRollingBenchmarkProposalAdjuster(double Cap) : IRollingBenchmarkProposalStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingBenchmarkProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameRollingBenchmarkProposalAdjuster(Dictionary<string, double> Factors) : IRollingBenchmarkProposalStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RollingBenchmarkProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RollingBenchmarkProposalFteOffsetAdjuster(double Weight) : IRollingBenchmarkProposalStep
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(RollingBenchmarkProposal item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

/// <summary>Runs the rolling benchmark proposal steps in order and keeps a trace of each.</summary>
public sealed class RollingBenchmarkProposalEngine(List<IRollingBenchmarkProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingBenchmarkProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingBenchmarkProposalEngine DefaultRollingBenchmarkProposalEngine()
    {
        return new RollingBenchmarkProposalEngine(
            new List<IRollingBenchmarkProposalStep>
            {
                new PercentRollingBenchmarkProposalAdjuster(-4.7),
                new MinimumRollingBenchmarkProposalAdjuster(6.1),
                new CapRollingBenchmarkProposalAdjuster(42853.0),
                new NameRollingBenchmarkProposalAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.14, ["central"] = 1.25 }),
                new RollingBenchmarkProposalFteOffsetAdjuster(0.41),
            },
            new List<string>(),
            3.3,
            55938.4);
    }

    public double Run(RollingBenchmarkProposal item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RollingBenchmarkProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
