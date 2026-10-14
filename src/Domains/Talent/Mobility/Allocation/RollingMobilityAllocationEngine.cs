namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling mobility allocation calculation.</summary>
public interface IRollingMobilityAllocationAdjuster
{
    string Describe();
    double Adjust(RollingMobilityAllocation item, double valueValue);
}

public sealed record RoundingRollingMobilityAllocationAdjuster(double Step) : IRollingMobilityAllocationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingMobilityAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRollingMobilityAllocationAdjuster(double Percent) : IRollingMobilityAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingMobilityAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameRollingMobilityAllocationAdjuster(Dictionary<string, double> Factors) : IRollingMobilityAllocationAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RollingMobilityAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RollingMobilityAllocationGraceDaysOffsetAdjuster(double Weight) : IRollingMobilityAllocationAdjuster
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(RollingMobilityAllocation item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record CapRollingMobilityAllocationAdjuster(double Cap) : IRollingMobilityAllocationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingMobilityAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling mobility allocation steps in order and keeps a trace of each.</summary>
public sealed class RollingMobilityAllocationEngine(List<IRollingMobilityAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingMobilityAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingMobilityAllocationEngine DefaultRollingMobilityAllocationEngine()
    {
        return new RollingMobilityAllocationEngine(
            new List<IRollingMobilityAllocationAdjuster>
            {
                new RoundingRollingMobilityAllocationAdjuster(0.5),
                new PercentRollingMobilityAllocationAdjuster(-12.4),
                new NameRollingMobilityAllocationAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.1, ["south"] = 1.31, ["online"] = 1.38 }),
                new RollingMobilityAllocationGraceDaysOffsetAdjuster(0.75),
                new CapRollingMobilityAllocationAdjuster(14074.0),
            },
            new List<string>(),
            1.0,
            78083.9);
    }

    public double Evaluate(RollingMobilityAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<RollingMobilityAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
