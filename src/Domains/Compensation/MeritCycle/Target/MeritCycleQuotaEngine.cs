namespace Acme.Workforce.Compensation.MeritCycle.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle quota calculation.</summary>
public interface IMeritCycleQuotaAdjuster
{
    string Describe();
    double Adjust(MeritCycleQuota item, double valueValue);
}

public sealed record CapMeritCycleQuotaAdjuster(double Cap) : IMeritCycleQuotaAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MeritCycleQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CategoryMeritCycleQuotaAdjuster(Dictionary<string, double> Factors) : IMeritCycleQuotaAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(MeritCycleQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentMeritCycleQuotaAdjuster(double Percent) : IMeritCycleQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MeritCycleQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MeritCycleQuotaScoreOffsetAdjuster(double Weight) : IMeritCycleQuotaAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(MeritCycleQuota item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record RoundingMeritCycleQuotaAdjuster(double Step) : IMeritCycleQuotaAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MeritCycleQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedMeritCycleQuotaAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the merit cycle quota steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleQuotaEngine(List<IMeritCycleQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleQuotaEngine DefaultMeritCycleQuotaEngine(Registry flags)
    {
        return new MeritCycleQuotaEngine(
            new List<IMeritCycleQuotaAdjuster>
            {
                new CapMeritCycleQuotaAdjuster(47366.0),
                new CategoryMeritCycleQuotaAdjuster(new Dictionary<string, double> { ["north"] = 0.74 }),
                new PercentMeritCycleQuotaAdjuster(-25.0),
                new MeritCycleQuotaScoreOffsetAdjuster(0.08),
                new RoundingMeritCycleQuotaAdjuster(0.5),
                new FlaggedMeritCycleQuotaAdjuster(flags, "merit-cycle-quota-scheduled", -6.8),
            },
            new List<string>(),
            1.1,
            36579.7);
    }

    public double Evaluate(MeritCycleQuota item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
