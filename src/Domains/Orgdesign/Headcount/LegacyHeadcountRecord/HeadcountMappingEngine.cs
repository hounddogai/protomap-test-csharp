namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount mapping calculation.</summary>
public interface IHeadcountMappingStep
{
    string Describe();
    double Adjust(HeadcountMapping item, double valueValue);
}

public sealed record FlaggedHeadcountMappingAdjuster(Registry Flags, string Flag, double Percent) : IHeadcountMappingStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HeadcountMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record HeadcountMappingSequenceOffsetAdjuster(double Weight) : IHeadcountMappingStep
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(HeadcountMapping item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record PercentHeadcountMappingAdjuster(double Percent) : IHeadcountMappingStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HeadcountMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelHeadcountMappingAdjuster(Dictionary<string, double> Factors) : IHeadcountMappingStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(HeadcountMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapHeadcountMappingAdjuster(double Cap) : IHeadcountMappingStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HeadcountMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumHeadcountMappingAdjuster(double Minimum) : IHeadcountMappingStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HeadcountMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the headcount mapping steps in order and keeps a trace of each.</summary>
public sealed class HeadcountMappingEngine(List<IHeadcountMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountMappingEngine DefaultHeadcountMappingEngine(Registry flags)
    {
        return new HeadcountMappingEngine(
            new List<IHeadcountMappingStep>
            {
                new FlaggedHeadcountMappingAdjuster(flags, "headcount-mapping-global", -1.9),
                new HeadcountMappingSequenceOffsetAdjuster(1.34),
                new PercentHeadcountMappingAdjuster(13.8),
                new LabelHeadcountMappingAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.13, ["retail"] = 1.4 }),
                new CapHeadcountMappingAdjuster(47322.0),
                new MinimumHeadcountMappingAdjuster(32.6),
            },
            new List<string>(),
            1.3,
            59867.3);
    }

    public double Evaluate(HeadcountMapping item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
