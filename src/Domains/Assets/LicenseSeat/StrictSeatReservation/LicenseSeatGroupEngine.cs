namespace Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat group calculation.</summary>
public interface ILicenseSeatGroupFactor
{
    string Describe();
    double Adjust(LicenseSeatGroup item, double valueValue);
}

public sealed record RegionLicenseSeatGroupAdjuster(Dictionary<string, double> Factors) : ILicenseSeatGroupFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(LicenseSeatGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLicenseSeatGroupAdjuster(List<double> Bounds, List<double> Rates) : ILicenseSeatGroupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LicenseSeatGroup item, double valueValue)
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

public sealed record LicenseSeatGroupSequenceOffsetAdjuster(double Weight) : ILicenseSeatGroupFactor
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(LicenseSeatGroup item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record CapLicenseSeatGroupAdjuster(double Cap) : ILicenseSeatGroupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLicenseSeatGroupAdjuster(double Step) : ILicenseSeatGroupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LicenseSeatGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLicenseSeatGroupAdjuster(double Percent) : ILicenseSeatGroupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LicenseSeatGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the license seat group steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatGroupEngine(List<ILicenseSeatGroupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatGroupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatGroupEngine DefaultLicenseSeatGroupEngine()
    {
        return new LicenseSeatGroupEngine(
            new List<ILicenseSeatGroupFactor>
            {
                new RegionLicenseSeatGroupAdjuster(new Dictionary<string, double> { ["south"] = 0.8 }),
                new TieredLicenseSeatGroupAdjuster(
                    new List<double> { 519.0, 5592.0 },
                    new List<double> { 1.12, 1.17 }),
                new LicenseSeatGroupSequenceOffsetAdjuster(0.26),
                new CapLicenseSeatGroupAdjuster(17231.0),
                new RoundingLicenseSeatGroupAdjuster(5.0),
                new PercentLicenseSeatGroupAdjuster(2.0),
            },
            new List<string>(),
            4.0,
            72570.5);
    }

    public double Evaluate(LicenseSeatGroup item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
