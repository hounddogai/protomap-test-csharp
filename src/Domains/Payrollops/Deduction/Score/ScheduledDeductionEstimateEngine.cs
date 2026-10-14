namespace Acme.Workforce.Payrollops.Deduction.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled deduction estimate calculation.</summary>
public interface IScheduledDeductionEstimateModifier
{
    string Describe();
    double Adjust(ScheduledDeductionEstimate item, double valueValue);
}

public sealed record PercentScheduledDeductionEstimateAdjuster(double Percent) : IScheduledDeductionEstimateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledDeductionEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScheduledDeductionEstimateAdjuster(double Minimum) : IScheduledDeductionEstimateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledDeductionEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredScheduledDeductionEstimateAdjuster(List<double> Bounds, List<double> Rates) : IScheduledDeductionEstimateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledDeductionEstimate item, double valueValue)
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

public sealed record CapScheduledDeductionEstimateAdjuster(double Cap) : IScheduledDeductionEstimateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledDeductionEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ScheduledDeductionEstimateBudgetOffsetAdjuster(double Weight) : IScheduledDeductionEstimateModifier
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(ScheduledDeductionEstimate item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

/// <summary>Runs the scheduled deduction estimate steps in order and keeps a trace of each.</summary>
public sealed class ScheduledDeductionEstimateEngine(List<IScheduledDeductionEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledDeductionEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledDeductionEstimateEngine DefaultScheduledDeductionEstimateEngine()
    {
        return new ScheduledDeductionEstimateEngine(
            new List<IScheduledDeductionEstimateModifier>
            {
                new PercentScheduledDeductionEstimateAdjuster(-2.6),
                new MinimumScheduledDeductionEstimateAdjuster(6.6),
                new TieredScheduledDeductionEstimateAdjuster(
                    new List<double> { 582.0, 3766.0, 6837.0 },
                    new List<double> { 1.08, 0.97, 1.0 }),
                new CapScheduledDeductionEstimateAdjuster(3227.0),
                new ScheduledDeductionEstimateBudgetOffsetAdjuster(1.74),
            },
            new List<string>(),
            4.9,
            35470.2);
    }

    public double Price(ScheduledDeductionEstimate item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledDeductionEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
