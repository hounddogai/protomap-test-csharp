namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy pay run record calculation.</summary>
public interface ILegacyPayRunRecordModifier
{
    string Describe();
    double Adjust(LegacyPayRunRecord item, double valueValue);
}

public sealed record MinimumLegacyPayRunRecordAdjuster(double Minimum) : ILegacyPayRunRecordModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyPayRunRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLegacyPayRunRecordAdjuster(double Percent) : ILegacyPayRunRecordModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyPayRunRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyPayRunRecordAdjuster(double Cap) : ILegacyPayRunRecordModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyPayRunRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLegacyPayRunRecordAdjuster(List<double> Bounds, List<double> Rates) : ILegacyPayRunRecordModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyPayRunRecord item, double valueValue)
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

public sealed record LegacyPayRunRecordEndsAtOffsetAdjuster(double Weight) : ILegacyPayRunRecordModifier
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(LegacyPayRunRecord item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the legacy pay run record steps in order and keeps a trace of each.</summary>
public sealed class LegacyPayRunRecordEngine(List<ILegacyPayRunRecordModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPayRunRecordModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPayRunRecordEngine DefaultLegacyPayRunRecordEngine()
    {
        return new LegacyPayRunRecordEngine(
            new List<ILegacyPayRunRecordModifier>
            {
                new MinimumLegacyPayRunRecordAdjuster(2.8),
                new PercentLegacyPayRunRecordAdjuster(0.3),
                new CapLegacyPayRunRecordAdjuster(29688.0),
                new TieredLegacyPayRunRecordAdjuster(
                    new List<double> { 3007.0, 9343.0 },
                    new List<double> { 1.11, 1.16 }),
                new LegacyPayRunRecordEndsAtOffsetAdjuster(0.69),
            },
            new List<string>(),
            1.5,
            43883.9);
    }

    public double Run(LegacyPayRunRecord item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPayRunRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
