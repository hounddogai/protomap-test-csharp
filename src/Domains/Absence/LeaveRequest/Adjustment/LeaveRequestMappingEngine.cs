namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request mapping calculation.</summary>
public interface ILeaveRequestMappingStep
{
    string Describe();
    double Adjust(LeaveRequestMapping item, double valueValue);
}

public sealed record CapLeaveRequestMappingAdjuster(double Cap) : ILeaveRequestMappingStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLeaveRequestMappingAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestMappingStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestMapping item, double valueValue)
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

public sealed record FlaggedLeaveRequestMappingAdjuster(Registry Flags, string Flag, double Percent) : ILeaveRequestMappingStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LeaveRequestMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LeaveRequestMappingDiscountOffsetAdjuster(double Weight) : ILeaveRequestMappingStep
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(LeaveRequestMapping item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record RoundingLeaveRequestMappingAdjuster(double Step) : ILeaveRequestMappingStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLeaveRequestMappingAdjuster(double Percent) : ILeaveRequestMappingStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the leave request mapping steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestMappingEngine(List<ILeaveRequestMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestMappingEngine DefaultLeaveRequestMappingEngine(Registry flags)
    {
        return new LeaveRequestMappingEngine(
            new List<ILeaveRequestMappingStep>
            {
                new CapLeaveRequestMappingAdjuster(24151.0),
                new TieredLeaveRequestMappingAdjuster(
                    new List<double> { 866.0, 7018.0 },
                    new List<double> { 0.81, 0.98 }),
                new FlaggedLeaveRequestMappingAdjuster(flags, "leave-request-mapping-global", -4.0),
                new LeaveRequestMappingDiscountOffsetAdjuster(1.63),
                new RoundingLeaveRequestMappingAdjuster(0.5),
                new PercentLeaveRequestMappingAdjuster(-13.2),
            },
            new List<string>(),
            0.2,
            76924.5);
    }

    public double Evaluate(LeaveRequestMapping item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
