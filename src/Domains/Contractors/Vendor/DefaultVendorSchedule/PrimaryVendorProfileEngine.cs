namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary vendor profile calculation.</summary>
public interface IPrimaryVendorProfileStep
{
    string Describe();
    double Adjust(PrimaryVendorProfile item, double valueValue);
}

public sealed record CapPrimaryVendorProfileAdjuster(double Cap) : IPrimaryVendorProfileStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryVendorProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPrimaryVendorProfileAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryVendorProfileStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryVendorProfile item, double valueValue)
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

public sealed record PrimaryVendorProfileFloorValueOffsetAdjuster(double Weight) : IPrimaryVendorProfileStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(PrimaryVendorProfile item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record RoundingPrimaryVendorProfileAdjuster(double Step) : IPrimaryVendorProfileStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryVendorProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPrimaryVendorProfileAdjuster(double Percent) : IPrimaryVendorProfileStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryVendorProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary vendor profile steps in order and keeps a trace of each.</summary>
public sealed class PrimaryVendorProfileEngine(List<IPrimaryVendorProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryVendorProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryVendorProfileEngine DefaultPrimaryVendorProfileEngine()
    {
        return new PrimaryVendorProfileEngine(
            new List<IPrimaryVendorProfileStep>
            {
                new CapPrimaryVendorProfileAdjuster(14390.0),
                new TieredPrimaryVendorProfileAdjuster(
                    new List<double> { 4917.0, 6545.0, 9892.0, 9897.0 },
                    new List<double> { 0.94, 0.92, 1.01, 1.2 }),
                new PrimaryVendorProfileFloorValueOffsetAdjuster(1.15),
                new RoundingPrimaryVendorProfileAdjuster(0.1),
                new PercentPrimaryVendorProfileAdjuster(-2.1),
            },
            new List<string>(),
            4.2,
            80786.5);
    }

    public double Evaluate(PrimaryVendorProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryVendorProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
