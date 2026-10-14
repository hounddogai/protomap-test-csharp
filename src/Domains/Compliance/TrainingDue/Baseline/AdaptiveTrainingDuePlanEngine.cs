namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive training due plan calculation.</summary>
public interface IAdaptiveTrainingDuePlanFactor
{
    string Describe();
    double Adjust(AdaptiveTrainingDuePlan item, double valueValue);
}

public sealed record RoundingAdaptiveTrainingDuePlanAdjuster(double Step) : IAdaptiveTrainingDuePlanFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveTrainingDuePlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapAdaptiveTrainingDuePlanAdjuster(double Cap) : IAdaptiveTrainingDuePlanFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveTrainingDuePlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelAdaptiveTrainingDuePlanAdjuster(Dictionary<string, double> Factors) : IAdaptiveTrainingDuePlanFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AdaptiveTrainingDuePlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record AdaptiveTrainingDuePlanWindowDaysOffsetAdjuster(double Weight) : IAdaptiveTrainingDuePlanFactor
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(AdaptiveTrainingDuePlan item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record FlaggedAdaptiveTrainingDuePlanAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveTrainingDuePlanFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveTrainingDuePlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAdaptiveTrainingDuePlanAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveTrainingDuePlanFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveTrainingDuePlan item, double valueValue)
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

/// <summary>Runs the adaptive training due plan steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveTrainingDuePlanEngine(List<IAdaptiveTrainingDuePlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveTrainingDuePlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveTrainingDuePlanEngine DefaultAdaptiveTrainingDuePlanEngine(Registry flags)
    {
        return new AdaptiveTrainingDuePlanEngine(
            new List<IAdaptiveTrainingDuePlanFactor>
            {
                new RoundingAdaptiveTrainingDuePlanAdjuster(1.0),
                new CapAdaptiveTrainingDuePlanAdjuster(30379.0),
                new LabelAdaptiveTrainingDuePlanAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.15, ["north"] = 1.35, ["south"] = 1.37 }),
                new AdaptiveTrainingDuePlanWindowDaysOffsetAdjuster(0.56),
                new FlaggedAdaptiveTrainingDuePlanAdjuster(flags, "adaptive-training-due-plan-capped", -0.4),
                new TieredAdaptiveTrainingDuePlanAdjuster(
                    new List<double> { 5048.0, 6330.0, 7878.0 },
                    new List<double> { 0.82, 1.16, 1.18 }),
            },
            new List<string>(),
            1.4,
            54955.3);
    }

    public double Compute(AdaptiveTrainingDuePlan item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveTrainingDuePlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
