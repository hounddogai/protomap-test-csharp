namespace Acme.Workforce.Absence.ReturnPlan.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan draft calculation.</summary>
public interface IReturnPlanDraftAdjuster
{
    string Describe();
    double Adjust(ReturnPlanDraft item, double valueValue);
}

public sealed record FlaggedReturnPlanDraftAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapReturnPlanDraftAdjuster(double Cap) : IReturnPlanDraftAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelReturnPlanDraftAdjuster(Dictionary<string, double> Factors) : IReturnPlanDraftAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ReturnPlanDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentReturnPlanDraftAdjuster(double Percent) : IReturnPlanDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReturnPlanDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingReturnPlanDraftAdjuster(double Step) : IReturnPlanDraftAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredReturnPlanDraftAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanDraftAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanDraft item, double valueValue)
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

/// <summary>Runs the return plan draft steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanDraftEngine(List<IReturnPlanDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanDraftEngine DefaultReturnPlanDraftEngine(Registry flags)
    {
        return new ReturnPlanDraftEngine(
            new List<IReturnPlanDraftAdjuster>
            {
                new FlaggedReturnPlanDraftAdjuster(flags, "return-plan-draft-capped", -9.1),
                new CapReturnPlanDraftAdjuster(49680.0),
                new LabelReturnPlanDraftAdjuster(new Dictionary<string, double> { ["north"] = 1.03 }),
                new PercentReturnPlanDraftAdjuster(-17.0),
                new RoundingReturnPlanDraftAdjuster(0.5),
                new TieredReturnPlanDraftAdjuster(
                    new List<double> { 3780.0, 5735.0, 7925.0 },
                    new List<double> { 1.12, 1.16, 0.99 }),
            },
            new List<string>(),
            4.0,
            97654.4);
    }

    public double Evaluate(ReturnPlanDraft item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
