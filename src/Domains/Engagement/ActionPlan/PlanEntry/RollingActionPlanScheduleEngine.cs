namespace Acme.Workforce.Engagement.ActionPlan.PlanEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling action plan schedule calculation.</summary>
public interface IRollingActionPlanScheduleFactor
{
    string Describe();
    double Adjust(RollingActionPlanSchedule item, double valueValue);
}

public sealed record MinimumRollingActionPlanScheduleAdjuster(double Minimum) : IRollingActionPlanScheduleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingActionPlanSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SourceRollingActionPlanScheduleAdjuster(Dictionary<string, double> Factors) : IRollingActionPlanScheduleFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(RollingActionPlanSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RollingActionPlanScheduleDiscountOffsetAdjuster(double Weight) : IRollingActionPlanScheduleFactor
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(RollingActionPlanSchedule item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record TieredRollingActionPlanScheduleAdjuster(List<double> Bounds, List<double> Rates) : IRollingActionPlanScheduleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingActionPlanSchedule item, double valueValue)
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

public sealed record RoundingRollingActionPlanScheduleAdjuster(double Step) : IRollingActionPlanScheduleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingActionPlanSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the rolling action plan schedule steps in order and keeps a trace of each.</summary>
public sealed class RollingActionPlanScheduleEngine(List<IRollingActionPlanScheduleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingActionPlanScheduleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingActionPlanScheduleEngine DefaultRollingActionPlanScheduleEngine()
    {
        return new RollingActionPlanScheduleEngine(
            new List<IRollingActionPlanScheduleFactor>
            {
                new MinimumRollingActionPlanScheduleAdjuster(30.7),
                new SourceRollingActionPlanScheduleAdjuster(new Dictionary<string, double> { ["north"] = 1.1 }),
                new RollingActionPlanScheduleDiscountOffsetAdjuster(1.18),
                new TieredRollingActionPlanScheduleAdjuster(
                    new List<double> { 688.0, 2105.0, 6624.0, 9177.0 },
                    new List<double> { 0.95, 0.8, 1.11, 1.14 }),
                new RoundingRollingActionPlanScheduleAdjuster(0.25),
            },
            new List<string>(),
            1.4,
            40510.1);
    }

    public double Compute(RollingActionPlanSchedule item)
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

    public Dictionary<string, double> EvaluateAll(List<RollingActionPlanSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
