namespace Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>One step of the legacy refresh record calculation.</summary>
public interface ILegacyRefreshRecordModifier
{
    string Describe();
    double Adjust(LegacyRefreshRecord item, double valueValue);
}

public sealed record TieredLegacyRefreshRecordAdjuster(List<double> Bounds, List<double> Rates) : ILegacyRefreshRecordModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyRefreshRecord item, double valueValue)
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

public sealed record LegacyRefreshRecordGraceDaysOffsetAdjuster(double Weight) : ILegacyRefreshRecordModifier
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(LegacyRefreshRecord item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record MinimumLegacyRefreshRecordAdjuster(double Minimum) : ILegacyRefreshRecordModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyRefreshRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLegacyRefreshRecordAdjuster(double Percent) : ILegacyRefreshRecordModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyRefreshRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyRefreshRecordAdjuster(double Cap) : ILegacyRefreshRecordModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyRefreshRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the legacy refresh record steps in order and keeps a trace of each.</summary>
public sealed class LegacyRefreshRecordEngine(List<ILegacyRefreshRecordModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyRefreshRecordModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyRefreshRecordEngine DefaultLegacyRefreshRecordEngine()
    {
        return new LegacyRefreshRecordEngine(
            new List<ILegacyRefreshRecordModifier>
            {
                new TieredLegacyRefreshRecordAdjuster(
                    new List<double> { 4692.0, 5208.0, 6740.0 },
                    new List<double> { 0.84, 1.06, 1.04 }),
                new LegacyRefreshRecordGraceDaysOffsetAdjuster(0.96),
                new MinimumLegacyRefreshRecordAdjuster(31.5),
                new PercentLegacyRefreshRecordAdjuster(-15.4),
                new CapLegacyRefreshRecordAdjuster(10314.0),
            },
            new List<string>(),
            1.3,
            38361.6);
    }

    public double Run(LegacyRefreshRecord item)
    {
        var current = item.AdjustedExposure();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<LegacyRefreshRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
