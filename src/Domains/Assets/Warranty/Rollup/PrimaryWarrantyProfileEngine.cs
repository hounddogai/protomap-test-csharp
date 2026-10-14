namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary warranty profile calculation.</summary>
public interface IPrimaryWarrantyProfileAdjuster
{
    string Describe();
    double Adjust(PrimaryWarrantyProfile item, double valueValue);
}

public sealed record RoundingPrimaryWarrantyProfileAdjuster(double Step) : IPrimaryWarrantyProfileAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryWarrantyProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PrimaryWarrantyProfileLeaveDaysOffsetAdjuster(double Weight) : IPrimaryWarrantyProfileAdjuster
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(PrimaryWarrantyProfile item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record MinimumPrimaryWarrantyProfileAdjuster(double Minimum) : IPrimaryWarrantyProfileAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryWarrantyProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the primary warranty profile steps in order and keeps a trace of each.</summary>
public sealed class PrimaryWarrantyProfileEngine(List<IPrimaryWarrantyProfileAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryWarrantyProfileAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryWarrantyProfileEngine DefaultPrimaryWarrantyProfileEngine()
    {
        return new PrimaryWarrantyProfileEngine(
            new List<IPrimaryWarrantyProfileAdjuster>
            {
                new RoundingPrimaryWarrantyProfileAdjuster(0.5),
                new PrimaryWarrantyProfileLeaveDaysOffsetAdjuster(1.97),
                new MinimumPrimaryWarrantyProfileAdjuster(31.8),
            },
            new List<string>(),
            1.5,
            35609.6);
    }

    public double Compute(PrimaryWarrantyProfile item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryWarrantyProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
