namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending exception assignment calculation.</summary>
public interface IPendingExceptionAssignmentModifier
{
    string Describe();
    double Adjust(PendingExceptionAssignment item, double valueValue);
}

public sealed record TieredPendingExceptionAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IPendingExceptionAssignmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingExceptionAssignment item, double valueValue)
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

public sealed record PercentPendingExceptionAssignmentAdjuster(double Percent) : IPendingExceptionAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingExceptionAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPendingExceptionAssignmentAdjuster(double Cap) : IPendingExceptionAssignmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingExceptionAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PendingExceptionAssignmentGraceDaysOffsetAdjuster(double Weight) : IPendingExceptionAssignmentModifier
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(PendingExceptionAssignment item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

/// <summary>Runs the pending exception assignment steps in order and keeps a trace of each.</summary>
public sealed class PendingExceptionAssignmentEngine(List<IPendingExceptionAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingExceptionAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingExceptionAssignmentEngine DefaultPendingExceptionAssignmentEngine()
    {
        return new PendingExceptionAssignmentEngine(
            new List<IPendingExceptionAssignmentModifier>
            {
                new TieredPendingExceptionAssignmentAdjuster(
                    new List<double> { 2255.0, 9063.0, 9810.0 },
                    new List<double> { 1.06, 0.94, 1.07 }),
                new PercentPendingExceptionAssignmentAdjuster(-15.4),
                new CapPendingExceptionAssignmentAdjuster(7459.0),
                new PendingExceptionAssignmentGraceDaysOffsetAdjuster(0.59),
            },
            new List<string>(),
            4.8,
            92871.1);
    }

    public double Compute(PendingExceptionAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingExceptionAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
