namespace Acme.Workforce.Payrollops.Garnishment.Plan;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the garnishment plan calculation.</summary>
public interface IGarnishmentPlanAdjuster
{
    string Describe();
    double Adjust(GarnishmentPlan item, double valueValue);
}

public sealed record NameGarnishmentPlanAdjuster(Dictionary<string, double> Factors) : IGarnishmentPlanAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(GarnishmentPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedGarnishmentPlanAdjuster(Registry Flags, string Flag, double Percent) : IGarnishmentPlanAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GarnishmentPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentGarnishmentPlanAdjuster(double Percent) : IGarnishmentPlanAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GarnishmentPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the garnishment plan steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentPlanEngine(List<IGarnishmentPlanAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentPlanAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentPlanEngine DefaultGarnishmentPlanEngine(Registry flags)
    {
        return new GarnishmentPlanEngine(
            new List<IGarnishmentPlanAdjuster>
            {
                new NameGarnishmentPlanAdjuster(new Dictionary<string, double> { ["north"] = 1.33, ["west"] = 0.74 }),
                new FlaggedGarnishmentPlanAdjuster(flags, "garnishment-plan-regional", 5.6),
                new PercentGarnishmentPlanAdjuster(5.7),
            },
            new List<string>(),
            1.2,
            15720.2);
    }

    public double Price(GarnishmentPlan item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
