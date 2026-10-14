namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy warranty proposal calculation.</summary>
public interface ILegacyWarrantyProposalModifier
{
    string Describe();
    double Adjust(LegacyWarrantyProposal item, double valueValue);
}

public sealed record CapLegacyWarrantyProposalAdjuster(double Cap) : ILegacyWarrantyProposalModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyWarrantyProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLegacyWarrantyProposalAdjuster(double Minimum) : ILegacyWarrantyProposalModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyWarrantyProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLegacyWarrantyProposalAdjuster(Registry Flags, string Flag, double Percent) : ILegacyWarrantyProposalModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyWarrantyProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLegacyWarrantyProposalAdjuster(double Step) : ILegacyWarrantyProposalModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyWarrantyProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLegacyWarrantyProposalAdjuster(List<double> Bounds, List<double> Rates) : ILegacyWarrantyProposalModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyWarrantyProposal item, double valueValue)
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

/// <summary>Runs the legacy warranty proposal steps in order and keeps a trace of each.</summary>
public sealed class LegacyWarrantyProposalEngine(List<ILegacyWarrantyProposalModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyWarrantyProposalModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyWarrantyProposalEngine DefaultLegacyWarrantyProposalEngine(Registry flags)
    {
        return new LegacyWarrantyProposalEngine(
            new List<ILegacyWarrantyProposalModifier>
            {
                new CapLegacyWarrantyProposalAdjuster(9630.0),
                new MinimumLegacyWarrantyProposalAdjuster(27.1),
                new FlaggedLegacyWarrantyProposalAdjuster(flags, "legacy-warranty-proposal-fallback", -8.1),
                new RoundingLegacyWarrantyProposalAdjuster(0.1),
                new TieredLegacyWarrantyProposalAdjuster(
                    new List<double> { 69.0, 2706.0 },
                    new List<double> { 0.99, 0.9 }),
            },
            new List<string>(),
            0.6,
            64669.3);
    }

    public double Run(LegacyWarrantyProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyWarrantyProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
