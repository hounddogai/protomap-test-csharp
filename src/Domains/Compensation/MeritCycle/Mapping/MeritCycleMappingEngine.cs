namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle mapping calculation.</summary>
public interface IMeritCycleMappingStep
{
    string Describe();
    double Adjust(MeritCycleMapping item, double valueValue);
}

public sealed record RoundingMeritCycleMappingAdjuster(double Step) : IMeritCycleMappingStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MeritCycleMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RegionMeritCycleMappingAdjuster(Dictionary<string, double> Factors) : IMeritCycleMappingStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(MeritCycleMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentMeritCycleMappingAdjuster(double Percent) : IMeritCycleMappingStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MeritCycleMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedMeritCycleMappingAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleMappingStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapMeritCycleMappingAdjuster(double Cap) : IMeritCycleMappingStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MeritCycleMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the merit cycle mapping steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleMappingEngine(List<IMeritCycleMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleMappingEngine DefaultMeritCycleMappingEngine(Registry flags)
    {
        return new MeritCycleMappingEngine(
            new List<IMeritCycleMappingStep>
            {
                new RoundingMeritCycleMappingAdjuster(0.1),
                new RegionMeritCycleMappingAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.85, ["west"] = 1.35, ["south"] = 1.25 }),
                new PercentMeritCycleMappingAdjuster(-12.9),
                new FlaggedMeritCycleMappingAdjuster(flags, "merit-cycle-mapping-manual", 0.8),
                new CapMeritCycleMappingAdjuster(7504.0),
            },
            new List<string>(),
            2.9,
            45570.0);
    }

    public double Price(MeritCycleMapping item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
