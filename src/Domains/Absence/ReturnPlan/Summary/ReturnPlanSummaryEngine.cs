namespace Acme.Workforce.Absence.ReturnPlan.Summary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the return plan summary calculation.</summary>
public interface IReturnPlanSummaryStep
{
    string Describe();
    double Adjust(ReturnPlanSummary item, double valueValue);
}

public sealed record CapReturnPlanSummaryAdjuster(double Cap) : IReturnPlanSummaryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentReturnPlanSummaryAdjuster(double Percent) : IReturnPlanSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReturnPlanSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedReturnPlanSummaryAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanSummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredReturnPlanSummaryAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanSummary item, double valueValue)
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

/// <summary>Runs the return plan summary steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanSummaryEngine(List<IReturnPlanSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanSummaryEngine DefaultReturnPlanSummaryEngine(Registry flags)
    {
        return new ReturnPlanSummaryEngine(
            new List<IReturnPlanSummaryStep>
            {
                new CapReturnPlanSummaryAdjuster(49477.0),
                new PercentReturnPlanSummaryAdjuster(-8.6),
                new FlaggedReturnPlanSummaryAdjuster(flags, "return-plan-summary-manual", -4.1),
                new TieredReturnPlanSummaryAdjuster(
                    new List<double> { 203.0, 7466.0, 7737.0 },
                    new List<double> { 0.99, 0.99, 0.8 }),
            },
            new List<string>(),
            2.4,
            45234.5);
    }

    public double Evaluate(ReturnPlanSummary item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
