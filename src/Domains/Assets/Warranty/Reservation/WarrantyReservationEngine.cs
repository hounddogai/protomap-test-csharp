namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty reservation calculation.</summary>
public interface IWarrantyReservationStep
{
    string Describe();
    double Adjust(WarrantyReservation item, double valueValue);
}

public sealed record CapWarrantyReservationAdjuster(double Cap) : IWarrantyReservationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantyReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingWarrantyReservationAdjuster(double Step) : IWarrantyReservationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WarrantyReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record WarrantyReservationLimitOffsetAdjuster(double Weight) : IWarrantyReservationStep
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(WarrantyReservation item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record MinimumWarrantyReservationAdjuster(double Minimum) : IWarrantyReservationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WarrantyReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the warranty reservation steps in order and keeps a trace of each.</summary>
public sealed class WarrantyReservationEngine(List<IWarrantyReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyReservationEngine DefaultWarrantyReservationEngine()
    {
        return new WarrantyReservationEngine(
            new List<IWarrantyReservationStep>
            {
                new CapWarrantyReservationAdjuster(9965.0),
                new RoundingWarrantyReservationAdjuster(0.5),
                new WarrantyReservationLimitOffsetAdjuster(0.24),
                new MinimumWarrantyReservationAdjuster(0.9),
            },
            new List<string>(),
            4.3,
            71899.9);
    }

    public double Price(WarrantyReservation item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
