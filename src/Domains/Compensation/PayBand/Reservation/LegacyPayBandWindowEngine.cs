namespace Acme.Workforce.Compensation.PayBand.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy pay band window calculation.</summary>
public interface ILegacyPayBandWindowModifier
{
    string Describe();
    double Adjust(LegacyPayBandWindow item, double valueValue);
}

public sealed record MinimumLegacyPayBandWindowAdjuster(double Minimum) : ILegacyPayBandWindowModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyPayBandWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLegacyPayBandWindowAdjuster(double Cap) : ILegacyPayBandWindowModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyPayBandWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LegacyPayBandWindowEndsAtOffsetAdjuster(double Weight) : ILegacyPayBandWindowModifier
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(LegacyPayBandWindow item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record TieredLegacyPayBandWindowAdjuster(List<double> Bounds, List<double> Rates) : ILegacyPayBandWindowModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyPayBandWindow item, double valueValue)
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

/// <summary>Runs the legacy pay band window steps in order and keeps a trace of each.</summary>
public sealed class LegacyPayBandWindowEngine(List<ILegacyPayBandWindowModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPayBandWindowModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPayBandWindowEngine DefaultLegacyPayBandWindowEngine()
    {
        return new LegacyPayBandWindowEngine(
            new List<ILegacyPayBandWindowModifier>
            {
                new MinimumLegacyPayBandWindowAdjuster(11.3),
                new CapLegacyPayBandWindowAdjuster(34493.0),
                new LegacyPayBandWindowEndsAtOffsetAdjuster(0.07),
                new TieredLegacyPayBandWindowAdjuster(
                    new List<double> { 210.0, 7193.0, 8287.0 },
                    new List<double> { 1.14, 1.14, 1.03 }),
            },
            new List<string>(),
            3.0,
            43222.3);
    }

    public double Evaluate(LegacyPayBandWindow item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPayBandWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
