namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy reorg summary calculation.</summary>
public interface ILegacyReorgSummaryModifier
{
    string Describe();
    double Adjust(LegacyReorgSummary item, double valueValue);
}

public sealed record MinimumLegacyReorgSummaryAdjuster(double Minimum) : ILegacyReorgSummaryModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyReorgSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLegacyReorgSummaryAdjuster(double Cap) : ILegacyReorgSummaryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyReorgSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LegacyReorgSummaryAmountOffsetAdjuster(double Weight) : ILegacyReorgSummaryModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(LegacyReorgSummary item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the legacy reorg summary steps in order and keeps a trace of each.</summary>
public sealed class LegacyReorgSummaryEngine(List<ILegacyReorgSummaryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyReorgSummaryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyReorgSummaryEngine DefaultLegacyReorgSummaryEngine()
    {
        return new LegacyReorgSummaryEngine(
            new List<ILegacyReorgSummaryModifier>
            {
                new MinimumLegacyReorgSummaryAdjuster(26.4),
                new CapLegacyReorgSummaryAdjuster(9785.0),
                new LegacyReorgSummaryAmountOffsetAdjuster(0.99),
            },
            new List<string>(),
            4.7,
            34149.5);
    }

    public double Price(LegacyReorgSummary item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyReorgSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
