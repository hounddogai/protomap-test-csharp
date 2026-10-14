namespace Acme.Workforce.Payrollops.Deduction.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction slot calculation.</summary>
public interface IDeductionSlotStep
{
    string Describe();
    double Adjust(DeductionSlot item, double valueValue);
}

public sealed record UnitDeductionSlotAdjuster(Dictionary<string, double> Factors) : IDeductionSlotStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(DeductionSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapDeductionSlotAdjuster(double Cap) : IDeductionSlotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeductionSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingDeductionSlotAdjuster(double Step) : IDeductionSlotStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeductionSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DeductionSlotPriorityOffsetAdjuster(double Weight) : IDeductionSlotStep
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(DeductionSlot item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

/// <summary>Runs the deduction slot steps in order and keeps a trace of each.</summary>
public sealed class DeductionSlotEngine(List<IDeductionSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionSlotEngine DefaultDeductionSlotEngine()
    {
        return new DeductionSlotEngine(
            new List<IDeductionSlotStep>
            {
                new UnitDeductionSlotAdjuster(new Dictionary<string, double> { ["west"] = 1.12 }),
                new CapDeductionSlotAdjuster(45015.0),
                new RoundingDeductionSlotAdjuster(0.05),
                new DeductionSlotPriorityOffsetAdjuster(0.53),
            },
            new List<string>(),
            2.5,
            53951.5);
    }

    public double Compute(DeductionSlot item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<DeductionSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
