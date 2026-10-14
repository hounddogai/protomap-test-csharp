namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan schedule calculation.</summary>
public interface ISuccessionPlanScheduleStep
{
    string Describe();
    double Adjust(SuccessionPlanSchedule item, double valueValue);
}

public sealed record CapSuccessionPlanScheduleAdjuster(double Cap) : ISuccessionPlanScheduleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SuccessionPlanSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredSuccessionPlanScheduleAdjuster(List<double> Bounds, List<double> Rates) : ISuccessionPlanScheduleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SuccessionPlanSchedule item, double valueValue)
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

public sealed record RoundingSuccessionPlanScheduleAdjuster(double Step) : ISuccessionPlanScheduleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentSuccessionPlanScheduleAdjuster(double Percent) : ISuccessionPlanScheduleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the succession plan schedule steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanScheduleEngine(List<ISuccessionPlanScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanScheduleEngine DefaultSuccessionPlanScheduleEngine()
    {
        return new SuccessionPlanScheduleEngine(
            new List<ISuccessionPlanScheduleStep>
            {
                new CapSuccessionPlanScheduleAdjuster(39908.0),
                new TieredSuccessionPlanScheduleAdjuster(
                    new List<double> { 3775.0, 9120.0 },
                    new List<double> { 0.87, 1.1 }),
                new RoundingSuccessionPlanScheduleAdjuster(0.05),
                new PercentSuccessionPlanScheduleAdjuster(20.3),
            },
            new List<string>(),
            4.6,
            92285.4);
    }

    public double Evaluate(SuccessionPlanSchedule item)
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
