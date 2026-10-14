namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment allocation calculation.</summary>
public interface IAdjustmentAllocationAdjuster
{
    string Describe();
    double Adjust(AdjustmentAllocation item, double valueValue);
}

public sealed record RoundingAdjustmentAllocationAdjuster(double Step) : IAdjustmentAllocationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdjustmentAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapAdjustmentAllocationAdjuster(double Cap) : IAdjustmentAllocationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdjustmentAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AdjustmentAllocationTenureMonthsOffsetAdjuster(double Weight) : IAdjustmentAllocationAdjuster
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(AdjustmentAllocation item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

/// <summary>Runs the adjustment allocation steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentAllocationEngine(List<IAdjustmentAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentAllocationEngine DefaultAdjustmentAllocationEngine()
    {
        return new AdjustmentAllocationEngine(
            new List<IAdjustmentAllocationAdjuster>
            {
                new RoundingAdjustmentAllocationAdjuster(0.5),
                new CapAdjustmentAllocationAdjuster(38111.0),
                new AdjustmentAllocationTenureMonthsOffsetAdjuster(0.12),
            },
            new List<string>(),
            4.2,
            2919.8);
    }

    public double Evaluate(AdjustmentAllocation item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
