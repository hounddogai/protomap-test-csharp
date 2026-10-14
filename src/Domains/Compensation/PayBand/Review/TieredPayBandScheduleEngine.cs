namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered pay band schedule calculation.</summary>
public interface ITieredPayBandScheduleModifier
{
    string Describe();
    double Adjust(TieredPayBandSchedule item, double valueValue);
}

public sealed record CapTieredPayBandScheduleAdjuster(double Cap) : ITieredPayBandScheduleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredPayBandSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTieredPayBandScheduleAdjuster(double Percent) : ITieredPayBandScheduleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredPayBandSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceTieredPayBandScheduleAdjuster(Dictionary<string, double> Factors) : ITieredPayBandScheduleModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(TieredPayBandSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredTieredPayBandScheduleAdjuster(List<double> Bounds, List<double> Rates) : ITieredPayBandScheduleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredPayBandSchedule item, double valueValue)
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

public sealed record RoundingTieredPayBandScheduleAdjuster(double Step) : ITieredPayBandScheduleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredPayBandSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the tiered pay band schedule steps in order and keeps a trace of each.</summary>
public sealed class TieredPayBandScheduleEngine(List<ITieredPayBandScheduleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredPayBandScheduleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredPayBandScheduleEngine DefaultTieredPayBandScheduleEngine()
    {
        return new TieredPayBandScheduleEngine(
            new List<ITieredPayBandScheduleModifier>
            {
                new CapTieredPayBandScheduleAdjuster(21352.0),
                new PercentTieredPayBandScheduleAdjuster(10.0),
                new SourceTieredPayBandScheduleAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.32, ["west"] = 0.88, ["south"] = 1.07 }),
                new TieredTieredPayBandScheduleAdjuster(
                    new List<double> { 3804.0, 4346.0, 7093.0 },
                    new List<double> { 1.05, 1.2, 1.01 }),
                new RoundingTieredPayBandScheduleAdjuster(0.5),
            },
            new List<string>(),
            2.1,
            33504.1);
    }

    public double Evaluate(TieredPayBandSchedule item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TieredPayBandSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
