namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy license seat proposal calculation.</summary>
public interface ILegacyLicenseSeatProposalModifier
{
    string Describe();
    double Adjust(LegacyLicenseSeatProposal item, double valueValue);
}

public sealed record TieredLegacyLicenseSeatProposalAdjuster(List<double> Bounds, List<double> Rates) : ILegacyLicenseSeatProposalModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyLicenseSeatProposal item, double valueValue)
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

public sealed record CapLegacyLicenseSeatProposalAdjuster(double Cap) : ILegacyLicenseSeatProposalModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyLicenseSeatProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLegacyLicenseSeatProposalAdjuster(double Step) : ILegacyLicenseSeatProposalModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyLicenseSeatProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLegacyLicenseSeatProposalAdjuster(Registry Flags, string Flag, double Percent) : ILegacyLicenseSeatProposalModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyLicenseSeatProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryLegacyLicenseSeatProposalAdjuster(Dictionary<string, double> Factors) : ILegacyLicenseSeatProposalModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(LegacyLicenseSeatProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentLegacyLicenseSeatProposalAdjuster(double Percent) : ILegacyLicenseSeatProposalModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyLicenseSeatProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy license seat proposal steps in order and keeps a trace of each.</summary>
public sealed class LegacyLicenseSeatProposalEngine(List<ILegacyLicenseSeatProposalModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyLicenseSeatProposalModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyLicenseSeatProposalEngine DefaultLegacyLicenseSeatProposalEngine(Registry flags)
    {
        return new LegacyLicenseSeatProposalEngine(
            new List<ILegacyLicenseSeatProposalModifier>
            {
                new TieredLegacyLicenseSeatProposalAdjuster(
                    new List<double> { 687.0, 8260.0, 9874.0 },
                    new List<double> { 0.91, 0.82, 1.19 }),
                new CapLegacyLicenseSeatProposalAdjuster(8454.0),
                new RoundingLegacyLicenseSeatProposalAdjuster(0.05),
                new FlaggedLegacyLicenseSeatProposalAdjuster(flags, "legacy-license-seat-proposal-tiered", 4.1),
                new CategoryLegacyLicenseSeatProposalAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.98, ["retail"] = 1.19, ["north"] = 0.7 }),
                new PercentLegacyLicenseSeatProposalAdjuster(3.3),
            },
            new List<string>(),
            1.0,
            50266.2);
    }

    public double Price(LegacyLicenseSeatProposal item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyLicenseSeatProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
