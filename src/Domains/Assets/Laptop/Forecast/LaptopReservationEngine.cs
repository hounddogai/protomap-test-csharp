namespace Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>One step of the laptop reservation calculation.</summary>
public interface ILaptopReservationFactor
{
    string Describe();
    double Adjust(LaptopReservation item, double valueValue);
}

public sealed record CapLaptopReservationAdjuster(double Cap) : ILaptopReservationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DepartmentLaptopReservationAdjuster(Dictionary<string, double> Factors) : ILaptopReservationFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(LaptopReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLaptopReservationAdjuster(List<double> Bounds, List<double> Rates) : ILaptopReservationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LaptopReservation item, double valueValue)
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

public sealed record MinimumLaptopReservationAdjuster(double Minimum) : ILaptopReservationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LaptopReservationCeilingOffsetAdjuster(double Weight) : ILaptopReservationFactor
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(LaptopReservation item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

/// <summary>Runs the laptop reservation steps in order and keeps a trace of each.</summary>
public sealed class LaptopReservationEngine(List<ILaptopReservationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopReservationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopReservationEngine DefaultLaptopReservationEngine()
    {
        return new LaptopReservationEngine(
            new List<ILaptopReservationFactor>
            {
                new CapLaptopReservationAdjuster(16255.0),
                new DepartmentLaptopReservationAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.21, ["west"] = 1.01 }),
                new TieredLaptopReservationAdjuster(
                    new List<double> { 850.0, 7701.0 },
                    new List<double> { 1.06, 0.84 }),
                new MinimumLaptopReservationAdjuster(21.9),
                new LaptopReservationCeilingOffsetAdjuster(1.94),
            },
            new List<string>(),
            3.6,
            94362.2);
    }

    public double Evaluate(LaptopReservation item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
