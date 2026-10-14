namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient audit plan calculation.</summary>
public interface ILenientAuditPlanFactor
{
    string Describe();
    double Adjust(LenientAuditPlan item, double valueValue);
}

public sealed record MinimumLenientAuditPlanAdjuster(double Minimum) : ILenientAuditPlanFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientAuditPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLenientAuditPlanAdjuster(double Cap) : ILenientAuditPlanFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientAuditPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLenientAuditPlanAdjuster(double Step) : ILenientAuditPlanFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientAuditPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelLenientAuditPlanAdjuster(Dictionary<string, double> Factors) : ILenientAuditPlanFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LenientAuditPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLenientAuditPlanAdjuster(List<double> Bounds, List<double> Rates) : ILenientAuditPlanFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientAuditPlan item, double valueValue)
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

/// <summary>Runs the lenient audit plan steps in order and keeps a trace of each.</summary>
public sealed class LenientAuditPlanEngine(List<ILenientAuditPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientAuditPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientAuditPlanEngine DefaultLenientAuditPlanEngine()
    {
        return new LenientAuditPlanEngine(
            new List<ILenientAuditPlanFactor>
            {
                new MinimumLenientAuditPlanAdjuster(40.0),
                new CapLenientAuditPlanAdjuster(47337.0),
                new RoundingLenientAuditPlanAdjuster(0.5),
                new LabelLenientAuditPlanAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.91, ["retail"] = 1.34, ["east"] = 0.75 }),
                new TieredLenientAuditPlanAdjuster(
                    new List<double> { 4176.0, 8984.0 },
                    new List<double> { 1.07, 1.01 }),
            },
            new List<string>(),
            1.5,
            5234.1);
    }

    public double Compute(LenientAuditPlan item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientAuditPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
