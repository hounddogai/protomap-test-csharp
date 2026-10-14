namespace Acme.Workforce.Absence.ReturnPlan.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending return plan window calculation.</summary>
public interface IPendingReturnPlanWindowStep
{
    string Describe();
    double Adjust(PendingReturnPlanWindow item, double valueValue);
}

public sealed record PendingReturnPlanWindowSequenceOffsetAdjuster(double Weight) : IPendingReturnPlanWindowStep
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(PendingReturnPlanWindow item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record MinimumPendingReturnPlanWindowAdjuster(double Minimum) : IPendingReturnPlanWindowStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingReturnPlanWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPendingReturnPlanWindowAdjuster(List<double> Bounds, List<double> Rates) : IPendingReturnPlanWindowStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingReturnPlanWindow item, double valueValue)
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

public sealed record CapPendingReturnPlanWindowAdjuster(double Cap) : IPendingReturnPlanWindowStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingReturnPlanWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPendingReturnPlanWindowAdjuster(double Percent) : IPendingReturnPlanWindowStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingReturnPlanWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending return plan window steps in order and keeps a trace of each.</summary>
public sealed class PendingReturnPlanWindowEngine(List<IPendingReturnPlanWindowStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingReturnPlanWindowStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingReturnPlanWindowEngine DefaultPendingReturnPlanWindowEngine()
    {
        return new PendingReturnPlanWindowEngine(
            new List<IPendingReturnPlanWindowStep>
            {
                new PendingReturnPlanWindowSequenceOffsetAdjuster(0.37),
                new MinimumPendingReturnPlanWindowAdjuster(0.9),
                new TieredPendingReturnPlanWindowAdjuster(
                    new List<double> { 1162.0, 7773.0, 9940.0 },
                    new List<double> { 0.97, 0.91, 1.18 }),
                new CapPendingReturnPlanWindowAdjuster(5998.0),
                new PercentPendingReturnPlanWindowAdjuster(-5.0),
            },
            new List<string>(),
            0.7,
            51609.7);
    }

    public double Compute(PendingReturnPlanWindow item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingReturnPlanWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
