namespace Acme.Workforce.Workplace.Visitor.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor reservation calculation.</summary>
public interface IVisitorReservationStep
{
    string Describe();
    double Adjust(VisitorReservation item, double valueValue);
}

public sealed record FlaggedVisitorReservationAdjuster(Registry Flags, string Flag, double Percent) : IVisitorReservationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VisitorReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingVisitorReservationAdjuster(double Step) : IVisitorReservationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredVisitorReservationAdjuster(List<double> Bounds, List<double> Rates) : IVisitorReservationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorReservation item, double valueValue)
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

public sealed record CapVisitorReservationAdjuster(double Cap) : IVisitorReservationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleVisitorReservationAdjuster(Dictionary<string, double> Factors) : IVisitorReservationStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(VisitorReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record VisitorReservationCapacityOffsetAdjuster(double Weight) : IVisitorReservationStep
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(VisitorReservation item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

/// <summary>Runs the visitor reservation steps in order and keeps a trace of each.</summary>
public sealed class VisitorReservationEngine(List<IVisitorReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorReservationEngine DefaultVisitorReservationEngine(Registry flags)
    {
        return new VisitorReservationEngine(
            new List<IVisitorReservationStep>
            {
                new FlaggedVisitorReservationAdjuster(flags, "visitor-reservation-default", -5.8),
                new RoundingVisitorReservationAdjuster(0.05),
                new TieredVisitorReservationAdjuster(
                    new List<double> { 1166.0, 3817.0, 7035.0 },
                    new List<double> { 1.06, 0.95, 1.05 }),
                new CapVisitorReservationAdjuster(2838.0),
                new TitleVisitorReservationAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.17, ["west"] = 0.71 }),
                new VisitorReservationCapacityOffsetAdjuster(0.08),
            },
            new List<string>(),
            0.8,
            37947.4);
    }

    public double Evaluate(VisitorReservation item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
