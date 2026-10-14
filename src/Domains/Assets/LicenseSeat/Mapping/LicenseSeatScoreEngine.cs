namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat score calculation.</summary>
public interface ILicenseSeatScoreAdjuster
{
    string Describe();
    double Adjust(LicenseSeatScore item, double valueValue);
}

public sealed record TieredLicenseSeatScoreAdjuster(List<double> Bounds, List<double> Rates) : ILicenseSeatScoreAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LicenseSeatScore item, double valueValue)
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

public sealed record RoundingLicenseSeatScoreAdjuster(double Step) : ILicenseSeatScoreAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LicenseSeatScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitLicenseSeatScoreAdjuster(Dictionary<string, double> Factors) : ILicenseSeatScoreAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(LicenseSeatScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentLicenseSeatScoreAdjuster(double Percent) : ILicenseSeatScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LicenseSeatScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLicenseSeatScoreAdjuster(double Cap) : ILicenseSeatScoreAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the license seat score steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatScoreEngine(List<ILicenseSeatScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatScoreEngine DefaultLicenseSeatScoreEngine()
    {
        return new LicenseSeatScoreEngine(
            new List<ILicenseSeatScoreAdjuster>
            {
                new TieredLicenseSeatScoreAdjuster(
                    new List<double> { 1103.0, 2620.0, 7210.0 },
                    new List<double> { 1.16, 0.85, 1.2 }),
                new RoundingLicenseSeatScoreAdjuster(1.0),
                new UnitLicenseSeatScoreAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.96, ["east"] = 1.19 }),
                new PercentLicenseSeatScoreAdjuster(8.9),
                new CapLicenseSeatScoreAdjuster(25097.0),
            },
            new List<string>(),
            2.2,
            71170.8);
    }

    public double Price(LicenseSeatScore item)
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
