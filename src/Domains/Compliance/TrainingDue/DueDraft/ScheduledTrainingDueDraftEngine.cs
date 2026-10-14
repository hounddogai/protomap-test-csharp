namespace Acme.Workforce.Compliance.TrainingDue.DueDraft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled training due draft calculation.</summary>
public interface IScheduledTrainingDueDraftStep
{
    string Describe();
    double Adjust(ScheduledTrainingDueDraft item, double valueValue);
}

public sealed record PercentScheduledTrainingDueDraftAdjuster(double Percent) : IScheduledTrainingDueDraftStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledTrainingDueDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ScheduledTrainingDueDraftLimitOffsetAdjuster(double Weight) : IScheduledTrainingDueDraftStep
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(ScheduledTrainingDueDraft item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record TieredScheduledTrainingDueDraftAdjuster(List<double> Bounds, List<double> Rates) : IScheduledTrainingDueDraftStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledTrainingDueDraft item, double valueValue)
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

public sealed record RoundingScheduledTrainingDueDraftAdjuster(double Step) : IScheduledTrainingDueDraftStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledTrainingDueDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled training due draft steps in order and keeps a trace of each.</summary>
public sealed class ScheduledTrainingDueDraftEngine(List<IScheduledTrainingDueDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledTrainingDueDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledTrainingDueDraftEngine DefaultScheduledTrainingDueDraftEngine()
    {
        return new ScheduledTrainingDueDraftEngine(
            new List<IScheduledTrainingDueDraftStep>
            {
                new PercentScheduledTrainingDueDraftAdjuster(-15.7),
                new ScheduledTrainingDueDraftLimitOffsetAdjuster(0.98),
                new TieredScheduledTrainingDueDraftAdjuster(
                    new List<double> { 3653.0, 5939.0 },
                    new List<double> { 0.85, 0.91 }),
                new RoundingScheduledTrainingDueDraftAdjuster(0.25),
            },
            new List<string>(),
            2.1,
            18450.1);
    }

    public double Compute(ScheduledTrainingDueDraft item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledTrainingDueDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
