namespace Acme.Workforce.Contractors.Rate.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled rate allocation calculation.</summary>
public interface IScheduledRateAllocationAdjuster
{
    string Describe();
    double Adjust(ScheduledRateAllocation item, double valueValue);
}

public sealed record PercentScheduledRateAllocationAdjuster(double Percent) : IScheduledRateAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledRateAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ScheduledRateAllocationPriorityOffsetAdjuster(double Weight) : IScheduledRateAllocationAdjuster
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(ScheduledRateAllocation item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record RoundingScheduledRateAllocationAdjuster(double Step) : IScheduledRateAllocationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledRateAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled rate allocation steps in order and keeps a trace of each.</summary>
public sealed class ScheduledRateAllocationEngine(List<IScheduledRateAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledRateAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledRateAllocationEngine DefaultScheduledRateAllocationEngine()
    {
        return new ScheduledRateAllocationEngine(
            new List<IScheduledRateAllocationAdjuster>
            {
                new PercentScheduledRateAllocationAdjuster(-5.5),
                new ScheduledRateAllocationPriorityOffsetAdjuster(1.96),
                new RoundingScheduledRateAllocationAdjuster(0.25),
            },
            new List<string>(),
            2.8,
            54801.0);
    }

    public double Evaluate(ScheduledRateAllocation item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledRateAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
