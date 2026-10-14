namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending recognition allocation calculation.</summary>
public interface IPendingRecognitionAllocationAdjuster
{
    string Describe();
    double Adjust(PendingRecognitionAllocation item, double valueValue);
}

public sealed record LabelPendingRecognitionAllocationAdjuster(Dictionary<string, double> Factors) : IPendingRecognitionAllocationAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PendingRecognitionAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPendingRecognitionAllocationAdjuster(double Step) : IPendingRecognitionAllocationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingRecognitionAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPendingRecognitionAllocationAdjuster(double Cap) : IPendingRecognitionAllocationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingRecognitionAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PendingRecognitionAllocationTenureMonthsOffsetAdjuster(double Weight) : IPendingRecognitionAllocationAdjuster
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(PendingRecognitionAllocation item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

/// <summary>Runs the pending recognition allocation steps in order and keeps a trace of each.</summary>
public sealed class PendingRecognitionAllocationEngine(List<IPendingRecognitionAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingRecognitionAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingRecognitionAllocationEngine DefaultPendingRecognitionAllocationEngine()
    {
        return new PendingRecognitionAllocationEngine(
            new List<IPendingRecognitionAllocationAdjuster>
            {
                new LabelPendingRecognitionAllocationAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.02, ["west"] = 1.07 }),
                new RoundingPendingRecognitionAllocationAdjuster(0.1),
                new CapPendingRecognitionAllocationAdjuster(16317.0),
                new PendingRecognitionAllocationTenureMonthsOffsetAdjuster(0.49),
            },
            new List<string>(),
            0.3,
            20824.2);
    }

    public double Evaluate(PendingRecognitionAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingRecognitionAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
