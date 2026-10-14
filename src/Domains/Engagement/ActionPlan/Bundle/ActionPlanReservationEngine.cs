namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan reservation calculation.</summary>
public interface IActionPlanReservationModifier
{
    string Describe();
    double Adjust(ActionPlanReservation item, double valueValue);
}

public sealed record CountryActionPlanReservationAdjuster(Dictionary<string, double> Factors) : IActionPlanReservationModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(ActionPlanReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredActionPlanReservationAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanReservationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanReservation item, double valueValue)
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

public sealed record RoundingActionPlanReservationAdjuster(double Step) : IActionPlanReservationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapActionPlanReservationAdjuster(double Cap) : IActionPlanReservationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ActionPlanReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the action plan reservation steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanReservationEngine(List<IActionPlanReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanReservationEngine DefaultActionPlanReservationEngine()
    {
        return new ActionPlanReservationEngine(
            new List<IActionPlanReservationModifier>
            {
                new CountryActionPlanReservationAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.25, ["south"] = 1.07, ["north"] = 1.26 }),
                new TieredActionPlanReservationAdjuster(
                    new List<double> { 62.0, 5491.0, 7334.0, 8709.0 },
                    new List<double> { 1.11, 0.91, 1.13, 1.08 }),
                new RoundingActionPlanReservationAdjuster(1.0),
                new CapActionPlanReservationAdjuster(6761.0),
            },
            new List<string>(),
            1.2,
            68184.1);
    }

    public double Run(ActionPlanReservation item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
