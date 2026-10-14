namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled leave request mapping calculation.</summary>
public interface IScheduledLeaveRequestMappingModifier
{
    string Describe();
    double Adjust(ScheduledLeaveRequestMapping item, double valueValue);
}

public sealed record FlaggedScheduledLeaveRequestMappingAdjuster(Registry Flags, string Flag, double Percent) : IScheduledLeaveRequestMappingModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledLeaveRequestMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ScheduledLeaveRequestMappingThresholdOffsetAdjuster(double Weight) : IScheduledLeaveRequestMappingModifier
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(ScheduledLeaveRequestMapping item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record TieredScheduledLeaveRequestMappingAdjuster(List<double> Bounds, List<double> Rates) : IScheduledLeaveRequestMappingModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledLeaveRequestMapping item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record CapScheduledLeaveRequestMappingAdjuster(double Cap) : IScheduledLeaveRequestMappingModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledLeaveRequestMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the scheduled leave request mapping steps in order and keeps a trace of each.</summary>
public sealed class ScheduledLeaveRequestMappingEngine(List<IScheduledLeaveRequestMappingModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledLeaveRequestMappingModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledLeaveRequestMappingEngine DefaultScheduledLeaveRequestMappingEngine(Registry flags)
    {
        return new ScheduledLeaveRequestMappingEngine(
            new List<IScheduledLeaveRequestMappingModifier>
            {
                new FlaggedScheduledLeaveRequestMappingAdjuster(
                    flags,
                    "scheduled-leave-request-mapping-primary",
                    7.7),
                new ScheduledLeaveRequestMappingThresholdOffsetAdjuster(1.58),
                new TieredScheduledLeaveRequestMappingAdjuster(
                    new List<double> { 3090.0, 7861.0 },
                    new List<double> { 1.12, 0.88 }),
                new CapScheduledLeaveRequestMappingAdjuster(13532.0),
            },
            new List<string>(),
            1.5,
            59774.9);
    }

    public double Price(ScheduledLeaveRequestMapping item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledLeaveRequestMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
