namespace Acme.Workforce.Orgdesign.Span.Review;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the regional span record calculation.</summary>
public interface IRegionalSpanRecordStep
{
    string Describe();
    double Adjust(RegionalSpanRecord item, double valueValue);
}

public sealed record ChannelRegionalSpanRecordAdjuster(Dictionary<string, double> Factors) : IRegionalSpanRecordStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(RegionalSpanRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRegionalSpanRecordAdjuster(double Percent) : IRegionalSpanRecordStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalSpanRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRegionalSpanRecordAdjuster(Registry Flags, string Flag, double Percent) : IRegionalSpanRecordStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalSpanRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRegionalSpanRecordAdjuster(double Minimum) : IRegionalSpanRecordStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalSpanRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRegionalSpanRecordAdjuster(double Cap) : IRegionalSpanRecordStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalSpanRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionalSpanRecordRatioOffsetAdjuster(double Weight) : IRegionalSpanRecordStep
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(RegionalSpanRecord item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

/// <summary>Runs the regional span record steps in order and keeps a trace of each.</summary>
public sealed class RegionalSpanRecordEngine(List<IRegionalSpanRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalSpanRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalSpanRecordEngine DefaultRegionalSpanRecordEngine(Registry flags)
    {
        return new RegionalSpanRecordEngine(
            new List<IRegionalSpanRecordStep>
            {
                new ChannelRegionalSpanRecordAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.23, ["retail"] = 0.92 }),
                new PercentRegionalSpanRecordAdjuster(2.8),
                new FlaggedRegionalSpanRecordAdjuster(flags, "regional-span-record-scheduled", 9.8),
                new MinimumRegionalSpanRecordAdjuster(12.5),
                new CapRegionalSpanRecordAdjuster(35682.0),
                new RegionalSpanRecordRatioOffsetAdjuster(0.6),
            },
            new List<string>(),
            2.1,
            72201.0);
    }

    public double Run(RegionalSpanRecord item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalSpanRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
