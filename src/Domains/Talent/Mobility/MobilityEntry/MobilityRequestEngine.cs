namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility request calculation.</summary>
public interface IMobilityRequestFactor
{
    string Describe();
    double Adjust(MobilityRequest item, double valueValue);
}

public sealed record RoundingMobilityRequestAdjuster(double Step) : IMobilityRequestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MobilityRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapMobilityRequestAdjuster(double Cap) : IMobilityRequestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilityRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MobilityRequestMinimumOffsetAdjuster(double Weight) : IMobilityRequestFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(MobilityRequest item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record FlaggedMobilityRequestAdjuster(Registry Flags, string Flag, double Percent) : IMobilityRequestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MobilityRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumMobilityRequestAdjuster(double Minimum) : IMobilityRequestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MobilityRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ChannelMobilityRequestAdjuster(Dictionary<string, double> Factors) : IMobilityRequestFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(MobilityRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the mobility request steps in order and keeps a trace of each.</summary>
public sealed class MobilityRequestEngine(List<IMobilityRequestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityRequestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityRequestEngine DefaultMobilityRequestEngine(Registry flags)
    {
        return new MobilityRequestEngine(
            new List<IMobilityRequestFactor>
            {
                new RoundingMobilityRequestAdjuster(0.5),
                new CapMobilityRequestAdjuster(48639.0),
                new MobilityRequestMinimumOffsetAdjuster(1.16),
                new FlaggedMobilityRequestAdjuster(flags, "mobility-request-adaptive", -6.4),
                new MinimumMobilityRequestAdjuster(34.2),
                new ChannelMobilityRequestAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.1, ["central"] = 1.26 }),
            },
            new List<string>(),
            2.7,
            83861.2);
    }

    public double Run(MobilityRequest item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
