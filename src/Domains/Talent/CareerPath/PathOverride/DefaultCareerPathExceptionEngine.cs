namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default career path exception calculation.</summary>
public interface IDefaultCareerPathExceptionFactor
{
    string Describe();
    double Adjust(DefaultCareerPathException item, double valueValue);
}

public sealed record RoundingDefaultCareerPathExceptionAdjuster(double Step) : IDefaultCareerPathExceptionFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultCareerPathException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedDefaultCareerPathExceptionAdjuster(Registry Flags, string Flag, double Percent) : IDefaultCareerPathExceptionFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultCareerPathException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentDefaultCareerPathExceptionAdjuster(double Percent) : IDefaultCareerPathExceptionFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultCareerPathException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDefaultCareerPathExceptionAdjuster(double Cap) : IDefaultCareerPathExceptionFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultCareerPathException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumDefaultCareerPathExceptionAdjuster(double Minimum) : IDefaultCareerPathExceptionFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultCareerPathException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default career path exception steps in order and keeps a trace of each.</summary>
public sealed class DefaultCareerPathExceptionEngine(List<IDefaultCareerPathExceptionFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultCareerPathExceptionFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultCareerPathExceptionEngine DefaultDefaultCareerPathExceptionEngine(Registry flags)
    {
        return new DefaultCareerPathExceptionEngine(
            new List<IDefaultCareerPathExceptionFactor>
            {
                new RoundingDefaultCareerPathExceptionAdjuster(0.05),
                new FlaggedDefaultCareerPathExceptionAdjuster(flags, "default-career-path-exception-primary", 0.6),
                new PercentDefaultCareerPathExceptionAdjuster(6.5),
                new CapDefaultCareerPathExceptionAdjuster(46951.0),
                new MinimumDefaultCareerPathExceptionAdjuster(38.2),
            },
            new List<string>(),
            3.4,
            19654.4);
    }

    public double Compute(DefaultCareerPathException item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultCareerPathException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
