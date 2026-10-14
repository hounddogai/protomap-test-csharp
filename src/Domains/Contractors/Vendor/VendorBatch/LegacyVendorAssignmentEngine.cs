namespace Acme.Workforce.Contractors.Vendor.VendorBatch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy vendor assignment calculation.</summary>
public interface ILegacyVendorAssignmentAdjuster
{
    string Describe();
    double Adjust(LegacyVendorAssignment item, double valueValue);
}

public sealed record PercentLegacyVendorAssignmentAdjuster(double Percent) : ILegacyVendorAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyVendorAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyVendorAssignmentAdjuster(double Cap) : ILegacyVendorAssignmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyVendorAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLegacyVendorAssignmentAdjuster(List<double> Bounds, List<double> Rates) : ILegacyVendorAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyVendorAssignment item, double valueValue)
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

public sealed record FlaggedLegacyVendorAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ILegacyVendorAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyVendorAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyVendorAssignmentAdjuster(double Minimum) : ILegacyVendorAssignmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyVendorAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLegacyVendorAssignmentAdjuster(double Step) : ILegacyVendorAssignmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyVendorAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the legacy vendor assignment steps in order and keeps a trace of each.</summary>
public sealed class LegacyVendorAssignmentEngine(List<ILegacyVendorAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyVendorAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyVendorAssignmentEngine DefaultLegacyVendorAssignmentEngine(Registry flags)
    {
        return new LegacyVendorAssignmentEngine(
            new List<ILegacyVendorAssignmentAdjuster>
            {
                new PercentLegacyVendorAssignmentAdjuster(22.6),
                new CapLegacyVendorAssignmentAdjuster(2339.0),
                new TieredLegacyVendorAssignmentAdjuster(
                    new List<double> { 2505.0, 5426.0, 6410.0, 7366.0 },
                    new List<double> { 1.03, 1.12, 0.83, 1.07 }),
                new FlaggedLegacyVendorAssignmentAdjuster(flags, "legacy-vendor-assignment-legacy", -5.0),
                new MinimumLegacyVendorAssignmentAdjuster(32.4),
                new RoundingLegacyVendorAssignmentAdjuster(0.25),
            },
            new List<string>(),
            4.3,
            10947.5);
    }

    public double Run(LegacyVendorAssignment item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyVendorAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
