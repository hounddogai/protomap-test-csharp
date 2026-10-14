namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band audit calculation.</summary>
public interface IPayBandAuditModifier
{
    string Describe();
    double Adjust(PayBandAudit item, double valueValue);
}

public sealed record CapPayBandAuditAdjuster(double Cap) : IPayBandAuditModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPayBandAuditAdjuster(double Step) : IPayBandAuditModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DepartmentPayBandAuditAdjuster(Dictionary<string, double> Factors) : IPayBandAuditModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PayBandAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pay band audit steps in order and keeps a trace of each.</summary>
public sealed class PayBandAuditEngine(List<IPayBandAuditModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandAuditModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandAuditEngine DefaultPayBandAuditEngine()
    {
        return new PayBandAuditEngine(
            new List<IPayBandAuditModifier>
            {
                new CapPayBandAuditAdjuster(41593.0),
                new RoundingPayBandAuditAdjuster(0.05),
                new DepartmentPayBandAuditAdjuster(new Dictionary<string, double> { ["retail"] = 1.05 }),
            },
            new List<string>(),
            2.3,
            97474.4);
    }

    public double Evaluate(PayBandAudit item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
