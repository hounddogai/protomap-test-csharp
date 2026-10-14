namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive garnishment plan calculation.</summary>
public interface IAdaptiveGarnishmentPlanModifier
{
    string Describe();
    double Adjust(AdaptiveGarnishmentPlan item, double valueValue);
}

public sealed record RoundingAdaptiveGarnishmentPlanAdjuster(double Step) : IAdaptiveGarnishmentPlanModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveGarnishmentPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapAdaptiveGarnishmentPlanAdjuster(double Cap) : IAdaptiveGarnishmentPlanModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveGarnishmentPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAdaptiveGarnishmentPlanAdjuster(double Minimum) : IAdaptiveGarnishmentPlanModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveGarnishmentPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAdaptiveGarnishmentPlanAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveGarnishmentPlanModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveGarnishmentPlan item, double valueValue)
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

/// <summary>Runs the adaptive garnishment plan steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveGarnishmentPlanEngine(List<IAdaptiveGarnishmentPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveGarnishmentPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveGarnishmentPlanEngine DefaultAdaptiveGarnishmentPlanEngine()
    {
        return new AdaptiveGarnishmentPlanEngine(
            new List<IAdaptiveGarnishmentPlanModifier>
            {
                new RoundingAdaptiveGarnishmentPlanAdjuster(0.5),
                new CapAdaptiveGarnishmentPlanAdjuster(13977.0),
                new MinimumAdaptiveGarnishmentPlanAdjuster(18.3),
                new TieredAdaptiveGarnishmentPlanAdjuster(
                    new List<double> { 684.0, 1296.0, 8300.0 },
                    new List<double> { 1.07, 1.12, 0.94 }),
            },
            new List<string>(),
            3.2,
            71326.8);
    }

    public double Price(AdaptiveGarnishmentPlan item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveGarnishmentPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
