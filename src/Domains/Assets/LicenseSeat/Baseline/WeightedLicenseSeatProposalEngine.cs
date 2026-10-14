namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the weighted license seat proposal calculation.</summary>
public interface IWeightedLicenseSeatProposalModifier
{
    string Describe();
    double Adjust(WeightedLicenseSeatProposal item, double valueValue);
}

public sealed record MinimumWeightedLicenseSeatProposalAdjuster(double Minimum) : IWeightedLicenseSeatProposalModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedLicenseSeatProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentWeightedLicenseSeatProposalAdjuster(double Percent) : IWeightedLicenseSeatProposalModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedLicenseSeatProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedWeightedLicenseSeatProposalAdjuster(Registry Flags, string Flag, double Percent) : IWeightedLicenseSeatProposalModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedLicenseSeatProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted license seat proposal steps in order and keeps a trace of each.</summary>
public sealed class WeightedLicenseSeatProposalEngine(List<IWeightedLicenseSeatProposalModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedLicenseSeatProposalModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedLicenseSeatProposalEngine DefaultWeightedLicenseSeatProposalEngine(Registry flags)
    {
        return new WeightedLicenseSeatProposalEngine(
            new List<IWeightedLicenseSeatProposalModifier>
            {
                new MinimumWeightedLicenseSeatProposalAdjuster(18.4),
                new PercentWeightedLicenseSeatProposalAdjuster(-15.2),
                new FlaggedWeightedLicenseSeatProposalAdjuster(flags, "weighted-license-seat-proposal-legacy", -2.0),
            },
            new List<string>(),
            1.3,
            97462.0);
    }

    public double Compute(WeightedLicenseSeatProposal item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedLicenseSeatProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
