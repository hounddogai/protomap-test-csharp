namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit override calculation.</summary>
public interface IAuditOverrideAdjuster
{
    string Describe();
    double Adjust(AuditOverride item, double valueValue);
}

public sealed record AuditOverrideVersionOffsetAdjuster(double Weight) : IAuditOverrideAdjuster
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(AuditOverride item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record PercentAuditOverrideAdjuster(double Percent) : IAuditOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAuditOverrideAdjuster(double Minimum) : IAuditOverrideAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AuditOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAuditOverrideAdjuster(double Cap) : IAuditOverrideAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AuditOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the audit override steps in order and keeps a trace of each.</summary>
public sealed class AuditOverrideEngine(List<IAuditOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditOverrideEngine DefaultAuditOverrideEngine()
    {
        return new AuditOverrideEngine(
            new List<IAuditOverrideAdjuster>
            {
                new AuditOverrideVersionOffsetAdjuster(1.29),
                new PercentAuditOverrideAdjuster(1.8),
                new MinimumAuditOverrideAdjuster(9.8),
                new CapAuditOverrideAdjuster(44595.0),
            },
            new List<string>(),
            3.7,
            20391.8);
    }

    public double Run(AuditOverride item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AuditOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
