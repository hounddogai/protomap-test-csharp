namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual target calculation.</summary>
public interface IAccrualTargetAdjuster
{
    string Describe();
    double Adjust(AccrualTarget item, double valueValue);
}

public sealed record CapAccrualTargetAdjuster(double Cap) : IAccrualTargetAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAccrualTargetAdjuster(List<double> Bounds, List<double> Rates) : IAccrualTargetAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccrualTarget item, double valueValue)
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

public sealed record PercentAccrualTargetAdjuster(double Percent) : IAccrualTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AccrualTargetRetriesOffsetAdjuster(double Weight) : IAccrualTargetAdjuster
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(AccrualTarget item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

/// <summary>Runs the accrual target steps in order and keeps a trace of each.</summary>
public sealed class AccrualTargetEngine(List<IAccrualTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualTargetEngine DefaultAccrualTargetEngine()
    {
        return new AccrualTargetEngine(
            new List<IAccrualTargetAdjuster>
            {
                new CapAccrualTargetAdjuster(48270.0),
                new TieredAccrualTargetAdjuster(
                    new List<double> { 1091.0, 4656.0, 7215.0, 8089.0 },
                    new List<double> { 1.0, 0.81, 0.82, 1.09 }),
                new PercentAccrualTargetAdjuster(-14.3),
                new AccrualTargetRetriesOffsetAdjuster(1.4),
            },
            new List<string>(),
            4.4,
            36308.0);
    }

    public double Compute(AccrualTarget item)
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

    public Dictionary<string, double> EvaluateAll(List<AccrualTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
