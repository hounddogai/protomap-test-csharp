namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted equity grant window calculation.</summary>
public interface IWeightedEquityGrantWindowModifier
{
    string Describe();
    double Adjust(WeightedEquityGrantWindow item, double valueValue);
}

public sealed record CapWeightedEquityGrantWindowAdjuster(double Cap) : IWeightedEquityGrantWindowModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedEquityGrantWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingWeightedEquityGrantWindowAdjuster(double Step) : IWeightedEquityGrantWindowModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedEquityGrantWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record WeightedEquityGrantWindowGraceDaysOffsetAdjuster(double Weight) : IWeightedEquityGrantWindowModifier
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(WeightedEquityGrantWindow item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record MinimumWeightedEquityGrantWindowAdjuster(double Minimum) : IWeightedEquityGrantWindowModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedEquityGrantWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedWeightedEquityGrantWindowAdjuster(Registry Flags, string Flag, double Percent) : IWeightedEquityGrantWindowModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedEquityGrantWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted equity grant window steps in order and keeps a trace of each.</summary>
public sealed class WeightedEquityGrantWindowEngine(List<IWeightedEquityGrantWindowModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedEquityGrantWindowModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedEquityGrantWindowEngine DefaultWeightedEquityGrantWindowEngine(Registry flags)
    {
        return new WeightedEquityGrantWindowEngine(
            new List<IWeightedEquityGrantWindowModifier>
            {
                new CapWeightedEquityGrantWindowAdjuster(48597.0),
                new RoundingWeightedEquityGrantWindowAdjuster(0.5),
                new WeightedEquityGrantWindowGraceDaysOffsetAdjuster(0.5),
                new MinimumWeightedEquityGrantWindowAdjuster(36.5),
                new FlaggedWeightedEquityGrantWindowAdjuster(flags, "weighted-equity-grant-window-seasonal", -1.9),
            },
            new List<string>(),
            4.7,
            39785.8);
    }

    public double Price(WeightedEquityGrantWindow item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedEquityGrantWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
