namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict pay band draft calculation.</summary>
public interface IStrictPayBandDraftAdjuster
{
    string Describe();
    double Adjust(StrictPayBandDraft item, double valueValue);
}

public sealed record StrictPayBandDraftFloorValueOffsetAdjuster(double Weight) : IStrictPayBandDraftAdjuster
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(StrictPayBandDraft item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record PercentStrictPayBandDraftAdjuster(double Percent) : IStrictPayBandDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictPayBandDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStrictPayBandDraftAdjuster(List<double> Bounds, List<double> Rates) : IStrictPayBandDraftAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictPayBandDraft item, double valueValue)
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

/// <summary>Runs the strict pay band draft steps in order and keeps a trace of each.</summary>
public sealed class StrictPayBandDraftEngine(List<IStrictPayBandDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPayBandDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPayBandDraftEngine DefaultStrictPayBandDraftEngine()
    {
        return new StrictPayBandDraftEngine(
            new List<IStrictPayBandDraftAdjuster>
            {
                new StrictPayBandDraftFloorValueOffsetAdjuster(1.18),
                new PercentStrictPayBandDraftAdjuster(9.9),
                new TieredStrictPayBandDraftAdjuster(
                    new List<double> { 1084.0, 2588.0 },
                    new List<double> { 1.05, 0.94 }),
            },
            new List<string>(),
            2.4,
            95731.9);
    }

    public double Price(StrictPayBandDraft item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StrictPayBandDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
