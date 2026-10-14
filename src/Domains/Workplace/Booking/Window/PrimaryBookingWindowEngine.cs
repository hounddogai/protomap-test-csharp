namespace Acme.Workforce.Workplace.Booking.Window;

/// <summary>One step of the primary booking window calculation.</summary>
public interface IPrimaryBookingWindowModifier
{
    string Describe();
    double Adjust(PrimaryBookingWindow item, double valueValue);
}

public sealed record CapPrimaryBookingWindowAdjuster(double Cap) : IPrimaryBookingWindowModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryBookingWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DepartmentPrimaryBookingWindowAdjuster(Dictionary<string, double> Factors) : IPrimaryBookingWindowModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PrimaryBookingWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPrimaryBookingWindowAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryBookingWindowModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryBookingWindow item, double valueValue)
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

/// <summary>Runs the primary booking window steps in order and keeps a trace of each.</summary>
public sealed class PrimaryBookingWindowEngine(List<IPrimaryBookingWindowModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryBookingWindowModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryBookingWindowEngine DefaultPrimaryBookingWindowEngine()
    {
        return new PrimaryBookingWindowEngine(
            new List<IPrimaryBookingWindowModifier>
            {
                new CapPrimaryBookingWindowAdjuster(3831.0),
                new DepartmentPrimaryBookingWindowAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.17, ["south"] = 1.11, ["east"] = 0.77 }),
                new TieredPrimaryBookingWindowAdjuster(
                    new List<double> { 3060.0, 3438.0 },
                    new List<double> { 0.95, 1.18 }),
            },
            new List<string>(),
            3.9,
            62683.7);
    }

    public double Run(PrimaryBookingWindow item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryBookingWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
