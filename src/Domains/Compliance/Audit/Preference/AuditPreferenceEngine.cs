namespace Acme.Workforce.Compliance.Audit.Preference;

/// <summary>One step of the audit preference calculation.</summary>
public interface IAuditPreferenceAdjuster
{
    string Describe();
    double Adjust(AuditPreference item, double valueValue);
}

public sealed record CapAuditPreferenceAdjuster(double Cap) : IAuditPreferenceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AuditPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AuditPreferenceHoursOffsetAdjuster(double Weight) : IAuditPreferenceAdjuster
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(AuditPreference item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record MinimumAuditPreferenceAdjuster(double Minimum) : IAuditPreferenceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AuditPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the audit preference steps in order and keeps a trace of each.</summary>
public sealed class AuditPreferenceEngine(List<IAuditPreferenceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditPreferenceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditPreferenceEngine DefaultAuditPreferenceEngine()
    {
        return new AuditPreferenceEngine(
            new List<IAuditPreferenceAdjuster>
            {
                new CapAuditPreferenceAdjuster(22859.0),
                new AuditPreferenceHoursOffsetAdjuster(1.55),
                new MinimumAuditPreferenceAdjuster(12.3),
            },
            new List<string>(),
            2.2,
            17712.9);
    }

    public double Evaluate(AuditPreference item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AuditPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
