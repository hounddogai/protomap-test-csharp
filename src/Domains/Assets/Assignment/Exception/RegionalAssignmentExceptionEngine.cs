namespace Acme.Workforce.Assets.Assignment.Exception;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the regional assignment exception calculation.</summary>
public interface IRegionalAssignmentExceptionStep
{
    string Describe();
    double Adjust(RegionalAssignmentException item, double valueValue);
}

public sealed record FlaggedRegionalAssignmentExceptionAdjuster(Registry Flags, string Flag, double Percent) : IRegionalAssignmentExceptionStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalAssignmentException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRegionalAssignmentExceptionAdjuster(double Cap) : IRegionalAssignmentExceptionStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalAssignmentException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionalAssignmentExceptionExpiresAtOffsetAdjuster(double Weight) : IRegionalAssignmentExceptionStep
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(RegionalAssignmentException item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record ChannelRegionalAssignmentExceptionAdjuster(Dictionary<string, double> Factors) : IRegionalAssignmentExceptionStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(RegionalAssignmentException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the regional assignment exception steps in order and keeps a trace of each.</summary>
public sealed class RegionalAssignmentExceptionEngine(List<IRegionalAssignmentExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalAssignmentExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalAssignmentExceptionEngine DefaultRegionalAssignmentExceptionEngine(Registry flags)
    {
        return new RegionalAssignmentExceptionEngine(
            new List<IRegionalAssignmentExceptionStep>
            {
                new FlaggedRegionalAssignmentExceptionAdjuster(flags, "regional-assignment-exception-manual", 6.4),
                new CapRegionalAssignmentExceptionAdjuster(23090.0),
                new RegionalAssignmentExceptionExpiresAtOffsetAdjuster(0.43),
                new ChannelRegionalAssignmentExceptionAdjuster(new Dictionary<string, double> { ["online"] = 1.38 }),
            },
            new List<string>(),
            2.4,
            79828.1);
    }

    public double Run(RegionalAssignmentException item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalAssignmentException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
