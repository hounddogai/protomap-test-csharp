namespace Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>One step of the warranty proposal calculation.</summary>
public interface IWarrantyProposalModifier
{
    string Describe();
    double Adjust(WarrantyProposal item, double valueValue);
}

public sealed record WarrantyProposalDiscountOffsetAdjuster(double Weight) : IWarrantyProposalModifier
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(WarrantyProposal item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record CapWarrantyProposalAdjuster(double Cap) : IWarrantyProposalModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantyProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumWarrantyProposalAdjuster(double Minimum) : IWarrantyProposalModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WarrantyProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the warranty proposal steps in order and keeps a trace of each.</summary>
public sealed class WarrantyProposalEngine(List<IWarrantyProposalModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyProposalModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyProposalEngine DefaultWarrantyProposalEngine()
    {
        return new WarrantyProposalEngine(
            new List<IWarrantyProposalModifier>
            {
                new WarrantyProposalDiscountOffsetAdjuster(0.31),
                new CapWarrantyProposalAdjuster(48145.0),
                new MinimumWarrantyProposalAdjuster(8.9),
            },
            new List<string>(),
            2.6,
            86006.7);
    }

    public double Run(WarrantyProposal item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
