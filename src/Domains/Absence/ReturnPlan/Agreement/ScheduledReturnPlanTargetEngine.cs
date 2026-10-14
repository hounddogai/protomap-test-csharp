namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled return plan target calculation.</summary>
public interface IScheduledReturnPlanTargetStep
{
    string Describe();
    double Adjust(ScheduledReturnPlanTarget item, double valueValue);
}

public sealed record MinimumScheduledReturnPlanTargetAdjuster(double Minimum) : IScheduledReturnPlanTargetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledReturnPlanTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentScheduledReturnPlanTargetAdjuster(double Percent) : IScheduledReturnPlanTargetStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledReturnPlanTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ScheduledReturnPlanTargetGraceDaysOffsetAdjuster(double Weight) : IScheduledReturnPlanTargetStep
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(ScheduledReturnPlanTarget item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record TieredScheduledReturnPlanTargetAdjuster(List<double> Bounds, List<double> Rates) : IScheduledReturnPlanTargetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledReturnPlanTarget item, double valueValue)
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

public sealed record ChannelScheduledReturnPlanTargetAdjuster(Dictionary<string, double> Factors) : IScheduledReturnPlanTargetStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(ScheduledReturnPlanTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingScheduledReturnPlanTargetAdjuster(double Step) : IScheduledReturnPlanTargetStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledReturnPlanTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled return plan target steps in order and keeps a trace of each.</summary>
public sealed class ScheduledReturnPlanTargetEngine(List<IScheduledReturnPlanTargetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledReturnPlanTargetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledReturnPlanTargetEngine DefaultScheduledReturnPlanTargetEngine()
    {
        return new ScheduledReturnPlanTargetEngine(
            new List<IScheduledReturnPlanTargetStep>
            {
                new MinimumScheduledReturnPlanTargetAdjuster(0.8),
                new PercentScheduledReturnPlanTargetAdjuster(-12.4),
                new ScheduledReturnPlanTargetGraceDaysOffsetAdjuster(0.45),
                new TieredScheduledReturnPlanTargetAdjuster(
                    new List<double> { 3810.0, 4800.0, 5701.0, 6106.0 },
                    new List<double> { 1.11, 1.08, 0.9, 1.03 }),
                new ChannelScheduledReturnPlanTargetAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.0, ["west"] = 0.81, ["central"] = 1.2 }),
                new RoundingScheduledReturnPlanTargetAdjuster(0.1),
            },
            new List<string>(),
            2.1,
            5688.2);
    }

    public double Price(ScheduledReturnPlanTarget item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledReturnPlanTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
