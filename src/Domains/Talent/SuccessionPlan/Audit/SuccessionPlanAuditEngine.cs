namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan audit calculation.</summary>
public interface ISuccessionPlanAuditStep
{
    string Describe();
    double Adjust(SuccessionPlanAudit item, double valueValue);
}

public sealed record TieredSuccessionPlanAuditAdjuster(List<double> Bounds, List<double> Rates) : ISuccessionPlanAuditStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SuccessionPlanAudit item, double valueValue)
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

public sealed record MinimumSuccessionPlanAuditAdjuster(double Minimum) : ISuccessionPlanAuditStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapSuccessionPlanAuditAdjuster(double Cap) : ISuccessionPlanAuditStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SuccessionPlanAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SuccessionPlanAuditSequenceOffsetAdjuster(double Weight) : ISuccessionPlanAuditStep
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(SuccessionPlanAudit item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

/// <summary>Runs the succession plan audit steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanAuditEngine(List<ISuccessionPlanAuditStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanAuditStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanAuditEngine DefaultSuccessionPlanAuditEngine()
    {
        return new SuccessionPlanAuditEngine(
            new List<ISuccessionPlanAuditStep>
            {
                new TieredSuccessionPlanAuditAdjuster(
                    new List<double> { 2035.0, 5466.0, 6457.0, 7737.0 },
                    new List<double> { 0.9, 1.18, 0.82, 1.11 }),
                new MinimumSuccessionPlanAuditAdjuster(21.7),
                new CapSuccessionPlanAuditAdjuster(46369.0),
                new SuccessionPlanAuditSequenceOffsetAdjuster(0.05),
            },
            new List<string>(),
            4.3,
            56938.8);
    }

    public double Run(SuccessionPlanAudit item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
