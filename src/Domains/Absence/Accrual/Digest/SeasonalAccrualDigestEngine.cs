namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal accrual digest calculation.</summary>
public interface ISeasonalAccrualDigestModifier
{
    string Describe();
    double Adjust(SeasonalAccrualDigest item, double valueValue);
}

public sealed record SeasonalAccrualDigestBudgetOffsetAdjuster(double Weight) : ISeasonalAccrualDigestModifier
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(SeasonalAccrualDigest item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record RoundingSeasonalAccrualDigestAdjuster(double Step) : ISeasonalAccrualDigestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalAccrualDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapSeasonalAccrualDigestAdjuster(double Cap) : ISeasonalAccrualDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalAccrualDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the seasonal accrual digest steps in order and keeps a trace of each.</summary>
public sealed class SeasonalAccrualDigestEngine(List<ISeasonalAccrualDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalAccrualDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalAccrualDigestEngine DefaultSeasonalAccrualDigestEngine()
    {
        return new SeasonalAccrualDigestEngine(
            new List<ISeasonalAccrualDigestModifier>
            {
                new SeasonalAccrualDigestBudgetOffsetAdjuster(1.69),
                new RoundingSeasonalAccrualDigestAdjuster(1.0),
                new CapSeasonalAccrualDigestAdjuster(48004.0),
            },
            new List<string>(),
            3.7,
            6894.5);
    }

    public double Run(SeasonalAccrualDigest item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalAccrualDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
