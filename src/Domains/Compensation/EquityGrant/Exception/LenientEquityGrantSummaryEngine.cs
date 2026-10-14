namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient equity grant summary calculation.</summary>
public interface ILenientEquityGrantSummaryStep
{
    string Describe();
    double Adjust(LenientEquityGrantSummary item, double valueValue);
}

public sealed record RoundingLenientEquityGrantSummaryAdjuster(double Step) : ILenientEquityGrantSummaryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientEquityGrantSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLenientEquityGrantSummaryAdjuster(Registry Flags, string Flag, double Percent) : ILenientEquityGrantSummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientEquityGrantSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientEquityGrantSummaryAdjuster(double Minimum) : ILenientEquityGrantSummaryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientEquityGrantSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredLenientEquityGrantSummaryAdjuster(List<double> Bounds, List<double> Rates) : ILenientEquityGrantSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientEquityGrantSummary item, double valueValue)
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

public sealed record LenientEquityGrantSummaryCostOffsetAdjuster(double Weight) : ILenientEquityGrantSummaryStep
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(LenientEquityGrantSummary item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

/// <summary>Runs the lenient equity grant summary steps in order and keeps a trace of each.</summary>
public sealed class LenientEquityGrantSummaryEngine(List<ILenientEquityGrantSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientEquityGrantSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientEquityGrantSummaryEngine DefaultLenientEquityGrantSummaryEngine(Registry flags)
    {
        return new LenientEquityGrantSummaryEngine(
            new List<ILenientEquityGrantSummaryStep>
            {
                new RoundingLenientEquityGrantSummaryAdjuster(0.1),
                new FlaggedLenientEquityGrantSummaryAdjuster(flags, "lenient-equity-grant-summary-tiered", 8.0),
                new MinimumLenientEquityGrantSummaryAdjuster(25.2),
                new TieredLenientEquityGrantSummaryAdjuster(
                    new List<double> { 749.0, 3052.0, 6120.0 },
                    new List<double> { 0.82, 0.86, 1.1 }),
                new LenientEquityGrantSummaryCostOffsetAdjuster(0.73),
            },
            new List<string>(),
            4.0,
            32057.0);
    }

    public double Compute(LenientEquityGrantSummary item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LenientEquityGrantSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
