namespace Acme.Workforce.Compensation.MeritCycle.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle audit calculation.</summary>
public interface IMeritCycleAuditStep
{
    string Describe();
    double Adjust(MeritCycleAudit item, double valueValue);
}

public sealed record PercentMeritCycleAuditAdjuster(double Percent) : IMeritCycleAuditStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MeritCycleAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingMeritCycleAuditAdjuster(double Step) : IMeritCycleAuditStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MeritCycleAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapMeritCycleAuditAdjuster(double Cap) : IMeritCycleAuditStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MeritCycleAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the merit cycle audit steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleAuditEngine(List<IMeritCycleAuditStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleAuditStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleAuditEngine DefaultMeritCycleAuditEngine()
    {
        return new MeritCycleAuditEngine(
            new List<IMeritCycleAuditStep>
            {
                new PercentMeritCycleAuditAdjuster(-17.8),
                new RoundingMeritCycleAuditAdjuster(5.0),
                new CapMeritCycleAuditAdjuster(28521.0),
            },
            new List<string>(),
            3.6,
            35083.4);
    }

    public double Run(MeritCycleAudit item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
