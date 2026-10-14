namespace Acme.Workforce.Compensation.Benchmark.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient benchmark proposal calculation.</summary>
public interface ILenientBenchmarkProposalFactor
{
    string Describe();
    double Adjust(LenientBenchmarkProposal item, double valueValue);
}

public sealed record TieredLenientBenchmarkProposalAdjuster(List<double> Bounds, List<double> Rates) : ILenientBenchmarkProposalFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientBenchmarkProposal item, double valueValue)
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

public sealed record ChannelLenientBenchmarkProposalAdjuster(Dictionary<string, double> Factors) : ILenientBenchmarkProposalFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(LenientBenchmarkProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LenientBenchmarkProposalBudgetOffsetAdjuster(double Weight) : ILenientBenchmarkProposalFactor
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(LenientBenchmarkProposal item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record RoundingLenientBenchmarkProposalAdjuster(double Step) : ILenientBenchmarkProposalFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientBenchmarkProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the lenient benchmark proposal steps in order and keeps a trace of each.</summary>
public sealed class LenientBenchmarkProposalEngine(List<ILenientBenchmarkProposalFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientBenchmarkProposalFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientBenchmarkProposalEngine DefaultLenientBenchmarkProposalEngine()
    {
        return new LenientBenchmarkProposalEngine(
            new List<ILenientBenchmarkProposalFactor>
            {
                new TieredLenientBenchmarkProposalAdjuster(
                    new List<double> { 3934.0, 6474.0, 8587.0 },
                    new List<double> { 0.95, 0.94, 0.98 }),
                new ChannelLenientBenchmarkProposalAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.28, ["online"] = 1.3 }),
                new LenientBenchmarkProposalBudgetOffsetAdjuster(0.09),
                new RoundingLenientBenchmarkProposalAdjuster(0.25),
            },
            new List<string>(),
            3.5,
            92700.5);
    }

    public double Evaluate(LenientBenchmarkProposal item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientBenchmarkProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
