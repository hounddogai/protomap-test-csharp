namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band claim calculation.</summary>
public interface IPayBandClaimFactor
{
    string Describe();
    double Adjust(PayBandClaim item, double valueValue);
}

public sealed record RoundingPayBandClaimAdjuster(double Step) : IPayBandClaimFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPayBandClaimAdjuster(double Percent) : IPayBandClaimFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPayBandClaimAdjuster(List<double> Bounds, List<double> Rates) : IPayBandClaimFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandClaim item, double valueValue)
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

public sealed record FlaggedPayBandClaimAdjuster(Registry Flags, string Flag, double Percent) : IPayBandClaimFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayBandClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pay band claim steps in order and keeps a trace of each.</summary>
public sealed class PayBandClaimEngine(List<IPayBandClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandClaimEngine DefaultPayBandClaimEngine(Registry flags)
    {
        return new PayBandClaimEngine(
            new List<IPayBandClaimFactor>
            {
                new RoundingPayBandClaimAdjuster(1.0),
                new PercentPayBandClaimAdjuster(-23.7),
                new TieredPayBandClaimAdjuster(new List<double> { 2942.0, 7154.0 }, new List<double> { 1.06, 0.97 }),
                new FlaggedPayBandClaimAdjuster(flags, "pay-band-claim-regional", 7.1),
            },
            new List<string>(),
            4.1,
            59588.0);
    }

    public double Run(PayBandClaim item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
