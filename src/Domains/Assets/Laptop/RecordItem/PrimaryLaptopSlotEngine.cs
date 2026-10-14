namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary laptop slot calculation.</summary>
public interface IPrimaryLaptopSlotStep
{
    string Describe();
    double Adjust(PrimaryLaptopSlot item, double valueValue);
}

public sealed record FlaggedPrimaryLaptopSlotAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryLaptopSlotStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryLaptopSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPrimaryLaptopSlotAdjuster(double Step) : IPrimaryLaptopSlotStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryLaptopSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPrimaryLaptopSlotAdjuster(double Cap) : IPrimaryLaptopSlotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryLaptopSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPrimaryLaptopSlotAdjuster(double Minimum) : IPrimaryLaptopSlotStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryLaptopSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DepartmentPrimaryLaptopSlotAdjuster(Dictionary<string, double> Factors) : IPrimaryLaptopSlotStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PrimaryLaptopSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the primary laptop slot steps in order and keeps a trace of each.</summary>
public sealed class PrimaryLaptopSlotEngine(List<IPrimaryLaptopSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryLaptopSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryLaptopSlotEngine DefaultPrimaryLaptopSlotEngine(Registry flags)
    {
        return new PrimaryLaptopSlotEngine(
            new List<IPrimaryLaptopSlotStep>
            {
                new FlaggedPrimaryLaptopSlotAdjuster(flags, "primary-laptop-slot-adaptive", 2.0),
                new RoundingPrimaryLaptopSlotAdjuster(0.1),
                new CapPrimaryLaptopSlotAdjuster(1845.0),
                new MinimumPrimaryLaptopSlotAdjuster(5.5),
                new DepartmentPrimaryLaptopSlotAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.29, ["south"] = 1.38 }),
            },
            new List<string>(),
            3.1,
            72405.5);
    }

    public double Run(PrimaryLaptopSlot item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryLaptopSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
