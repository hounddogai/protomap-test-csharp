namespace Acme.Workforce.Assets.Laptop.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop proposal calculation.</summary>
public interface ILaptopProposalFactor
{
    string Describe();
    double Adjust(LaptopProposal item, double valueValue);
}

public sealed record PercentLaptopProposalAdjuster(double Percent) : ILaptopProposalFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelLaptopProposalAdjuster(Dictionary<string, double> Factors) : ILaptopProposalFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(LaptopProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLaptopProposalAdjuster(double Minimum) : ILaptopProposalFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLaptopProposalAdjuster(double Step) : ILaptopProposalFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLaptopProposalAdjuster(List<double> Bounds, List<double> Rates) : ILaptopProposalFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LaptopProposal item, double valueValue)
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

public sealed record CapLaptopProposalAdjuster(double Cap) : ILaptopProposalFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the laptop proposal steps in order and keeps a trace of each.</summary>
public sealed class LaptopProposalEngine(List<ILaptopProposalFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopProposalFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopProposalEngine DefaultLaptopProposalEngine()
    {
        return new LaptopProposalEngine(
            new List<ILaptopProposalFactor>
            {
                new PercentLaptopProposalAdjuster(12.9),
                new ChannelLaptopProposalAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.3, ["central"] = 1.11, ["west"] = 0.93 }),
                new MinimumLaptopProposalAdjuster(31.2),
                new RoundingLaptopProposalAdjuster(5.0),
                new TieredLaptopProposalAdjuster(
                    new List<double> { 56.0, 1499.0, 6675.0, 7638.0 },
                    new List<double> { 1.06, 0.95, 0.98, 0.97 }),
                new CapLaptopProposalAdjuster(11019.0),
            },
            new List<string>(),
            1.5,
            43478.0);
    }

    public double Compute(LaptopProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<LaptopProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
