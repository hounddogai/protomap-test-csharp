namespace Acme.Workforce.Talent.Nomination.NominationRollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling nomination mapping calculation.</summary>
public interface IRollingNominationMappingStep
{
    string Describe();
    double Adjust(RollingNominationMapping item, double valueValue);
}

public sealed record FlaggedRollingNominationMappingAdjuster(Registry Flags, string Flag, double Percent) : IRollingNominationMappingStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingNominationMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRollingNominationMappingAdjuster(double Percent) : IRollingNominationMappingStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingNominationMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRollingNominationMappingAdjuster(double Cap) : IRollingNominationMappingStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingNominationMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelRollingNominationMappingAdjuster(Dictionary<string, double> Factors) : IRollingNominationMappingStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RollingNominationMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RollingNominationMappingSequenceOffsetAdjuster(double Weight) : IRollingNominationMappingStep
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(RollingNominationMapping item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

/// <summary>Runs the rolling nomination mapping steps in order and keeps a trace of each.</summary>
public sealed class RollingNominationMappingEngine(List<IRollingNominationMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingNominationMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingNominationMappingEngine DefaultRollingNominationMappingEngine(Registry flags)
    {
        return new RollingNominationMappingEngine(
            new List<IRollingNominationMappingStep>
            {
                new FlaggedRollingNominationMappingAdjuster(flags, "rolling-nomination-mapping-lenient", -9.6),
                new PercentRollingNominationMappingAdjuster(16.7),
                new CapRollingNominationMappingAdjuster(47738.0),
                new LabelRollingNominationMappingAdjuster(new Dictionary<string, double> { ["west"] = 0.93 }),
                new RollingNominationMappingSequenceOffsetAdjuster(1.97),
            },
            new List<string>(),
            0.7,
            51125.1);
    }

    public double Price(RollingNominationMapping item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingNominationMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
