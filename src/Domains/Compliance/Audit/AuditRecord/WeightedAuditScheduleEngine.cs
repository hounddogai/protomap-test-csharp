namespace Acme.Workforce.Compliance.Audit.AuditRecord;

/// <summary>One step of the weighted audit schedule calculation.</summary>
public interface IWeightedAuditScheduleFactor
{
    string Describe();
    double Adjust(WeightedAuditSchedule item, double valueValue);
}

public sealed record PercentWeightedAuditScheduleAdjuster(double Percent) : IWeightedAuditScheduleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedAuditSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumWeightedAuditScheduleAdjuster(double Minimum) : IWeightedAuditScheduleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedAuditSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record WeightedAuditScheduleExpiresAtOffsetAdjuster(double Weight) : IWeightedAuditScheduleFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(WeightedAuditSchedule item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

/// <summary>Runs the weighted audit schedule steps in order and keeps a trace of each.</summary>
public sealed class WeightedAuditScheduleEngine(List<IWeightedAuditScheduleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAuditScheduleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAuditScheduleEngine DefaultWeightedAuditScheduleEngine()
    {
        return new WeightedAuditScheduleEngine(
            new List<IWeightedAuditScheduleFactor>
            {
                new PercentWeightedAuditScheduleAdjuster(9.3),
                new MinimumWeightedAuditScheduleAdjuster(36.0),
                new WeightedAuditScheduleExpiresAtOffsetAdjuster(0.16),
            },
            new List<string>(),
            0.9,
            40540.1);
    }

    public double Price(WeightedAuditSchedule item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAuditSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
