namespace Acme.Workforce.Compliance.Audit.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit assignment calculation.</summary>
public interface IAuditAssignmentAdjuster
{
    string Describe();
    double Adjust(AuditAssignment item, double valueValue);
}

public sealed record MinimumAuditAssignmentAdjuster(double Minimum) : IAuditAssignmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AuditAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAuditAssignmentAdjuster(double Step) : IAuditAssignmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AuditAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAuditAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IAuditAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AuditAssignment item, double valueValue)
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

public sealed record PercentAuditAssignmentAdjuster(double Percent) : IAuditAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAuditAssignmentAdjuster(double Cap) : IAuditAssignmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AuditAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameAuditAssignmentAdjuster(Dictionary<string, double> Factors) : IAuditAssignmentAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AuditAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the audit assignment steps in order and keeps a trace of each.</summary>
public sealed class AuditAssignmentEngine(List<IAuditAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditAssignmentEngine DefaultAuditAssignmentEngine()
    {
        return new AuditAssignmentEngine(
            new List<IAuditAssignmentAdjuster>
            {
                new MinimumAuditAssignmentAdjuster(14.8),
                new RoundingAuditAssignmentAdjuster(0.05),
                new TieredAuditAssignmentAdjuster(
                    new List<double> { 2559.0, 2875.0, 4734.0, 7718.0 },
                    new List<double> { 1.09, 0.97, 0.93, 1.03 }),
                new PercentAuditAssignmentAdjuster(-8.2),
                new CapAuditAssignmentAdjuster(4293.0),
                new NameAuditAssignmentAdjuster(new Dictionary<string, double> { ["retail"] = 0.91 }),
            },
            new List<string>(),
            2.4,
            31813.8);
    }

    public double Price(AuditAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<AuditAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
