namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional warranty proposal calculation.</summary>
public interface IRegionalWarrantyProposalAdjuster
{
    string Describe();
    double Adjust(RegionalWarrantyProposal item, double valueValue);
}

public sealed record RegionalWarrantyProposalCreatedAtOffsetAdjuster(double Weight) : IRegionalWarrantyProposalAdjuster
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(RegionalWarrantyProposal item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record TieredRegionalWarrantyProposalAdjuster(List<double> Bounds, List<double> Rates) : IRegionalWarrantyProposalAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalWarrantyProposal item, double valueValue)
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

public sealed record CapRegionalWarrantyProposalAdjuster(double Cap) : IRegionalWarrantyProposalAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalWarrantyProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRegionalWarrantyProposalAdjuster(double Percent) : IRegionalWarrantyProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalWarrantyProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRegionalWarrantyProposalAdjuster(double Minimum) : IRegionalWarrantyProposalAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalWarrantyProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the regional warranty proposal steps in order and keeps a trace of each.</summary>
public sealed class RegionalWarrantyProposalEngine(List<IRegionalWarrantyProposalAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalWarrantyProposalAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalWarrantyProposalEngine DefaultRegionalWarrantyProposalEngine()
    {
        return new RegionalWarrantyProposalEngine(
            new List<IRegionalWarrantyProposalAdjuster>
            {
                new RegionalWarrantyProposalCreatedAtOffsetAdjuster(1.89),
                new TieredRegionalWarrantyProposalAdjuster(
                    new List<double> { 1120.0, 1962.0 },
                    new List<double> { 0.98, 1.13 }),
                new CapRegionalWarrantyProposalAdjuster(47719.0),
                new PercentRegionalWarrantyProposalAdjuster(-5.2),
                new MinimumRegionalWarrantyProposalAdjuster(5.4),
            },
            new List<string>(),
            3.4,
            99106.0);
    }

    public double Evaluate(RegionalWarrantyProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalWarrantyProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
