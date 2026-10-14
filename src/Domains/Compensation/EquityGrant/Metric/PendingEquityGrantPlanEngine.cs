namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending equity grant plan calculation.</summary>
public interface IPendingEquityGrantPlanFactor
{
    string Describe();
    double Adjust(PendingEquityGrantPlan item, double valueValue);
}

public sealed record PercentPendingEquityGrantPlanAdjuster(double Percent) : IPendingEquityGrantPlanFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingEquityGrantPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionPendingEquityGrantPlanAdjuster(Dictionary<string, double> Factors) : IPendingEquityGrantPlanFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(PendingEquityGrantPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PendingEquityGrantPlanWindowDaysOffsetAdjuster(double Weight) : IPendingEquityGrantPlanFactor
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(PendingEquityGrantPlan item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

/// <summary>Runs the pending equity grant plan steps in order and keeps a trace of each.</summary>
public sealed class PendingEquityGrantPlanEngine(List<IPendingEquityGrantPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingEquityGrantPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingEquityGrantPlanEngine DefaultPendingEquityGrantPlanEngine()
    {
        return new PendingEquityGrantPlanEngine(
            new List<IPendingEquityGrantPlanFactor>
            {
                new PercentPendingEquityGrantPlanAdjuster(-5.5),
                new RegionPendingEquityGrantPlanAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.91, ["south"] = 1.19, ["central"] = 0.97 }),
                new PendingEquityGrantPlanWindowDaysOffsetAdjuster(1.06),
            },
            new List<string>(),
            2.9,
            78077.8);
    }

    public double Run(PendingEquityGrantPlan item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingEquityGrantPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
