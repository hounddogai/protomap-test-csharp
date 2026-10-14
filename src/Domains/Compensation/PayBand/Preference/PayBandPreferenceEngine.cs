namespace Acme.Workforce.Compensation.PayBand.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band preference calculation.</summary>
public interface IPayBandPreferenceFactor
{
    string Describe();
    double Adjust(PayBandPreference item, double valueValue);
}

public sealed record PayBandPreferenceUpdatedAtOffsetAdjuster(double Weight) : IPayBandPreferenceFactor
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(PayBandPreference item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record PercentPayBandPreferenceAdjuster(double Percent) : IPayBandPreferenceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPayBandPreferenceAdjuster(double Step) : IPayBandPreferenceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPayBandPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IPayBandPreferenceFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandPreference item, double valueValue)
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

public sealed record CapPayBandPreferenceAdjuster(double Cap) : IPayBandPreferenceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pay band preference steps in order and keeps a trace of each.</summary>
public sealed class PayBandPreferenceEngine(List<IPayBandPreferenceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandPreferenceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandPreferenceEngine DefaultPayBandPreferenceEngine()
    {
        return new PayBandPreferenceEngine(
            new List<IPayBandPreferenceFactor>
            {
                new PayBandPreferenceUpdatedAtOffsetAdjuster(0.71),
                new PercentPayBandPreferenceAdjuster(-12.2),
                new RoundingPayBandPreferenceAdjuster(5.0),
                new TieredPayBandPreferenceAdjuster(
                    new List<double> { 2865.0, 6235.0 },
                    new List<double> { 1.1, 1.03 }),
                new CapPayBandPreferenceAdjuster(3507.0),
            },
            new List<string>(),
            2.9,
            86854.1);
    }

    public double Price(PayBandPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<PayBandPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
