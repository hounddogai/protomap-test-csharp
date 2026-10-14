namespace Acme.Workforce.Compensation.EquityGrant.Digest;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the equity grant digest calculation.</summary>
public interface IEquityGrantDigestStep
{
    string Describe();
    double Adjust(EquityGrantDigest item, double valueValue);
}

public sealed record FlaggedEquityGrantDigestAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantDigestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentEquityGrantDigestAdjuster(double Percent) : IEquityGrantDigestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EquityGrantDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record EquityGrantDigestDurationMinutesOffsetAdjuster(double Weight) : IEquityGrantDigestStep
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(EquityGrantDigest item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

/// <summary>Runs the equity grant digest steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantDigestEngine(List<IEquityGrantDigestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantDigestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantDigestEngine DefaultEquityGrantDigestEngine(Registry flags)
    {
        return new EquityGrantDigestEngine(
            new List<IEquityGrantDigestStep>
            {
                new FlaggedEquityGrantDigestAdjuster(flags, "equity-grant-digest-manual", 9.1),
                new PercentEquityGrantDigestAdjuster(-9.9),
                new EquityGrantDigestDurationMinutesOffsetAdjuster(0.94),
            },
            new List<string>(),
            4.7,
            19336.8);
    }

    public double Evaluate(EquityGrantDigest item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
