namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band template calculation.</summary>
public interface IPayBandTemplateAdjuster
{
    string Describe();
    double Adjust(PayBandTemplate item, double valueValue);
}

public sealed record PayBandTemplateHoursOffsetAdjuster(double Weight) : IPayBandTemplateAdjuster
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(PayBandTemplate item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record RoundingPayBandTemplateAdjuster(double Step) : IPayBandTemplateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPayBandTemplateAdjuster(double Percent) : IPayBandTemplateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPayBandTemplateAdjuster(double Cap) : IPayBandTemplateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPayBandTemplateAdjuster(Registry Flags, string Flag, double Percent) : IPayBandTemplateAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayBandTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPayBandTemplateAdjuster(double Minimum) : IPayBandTemplateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayBandTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pay band template steps in order and keeps a trace of each.</summary>
public sealed class PayBandTemplateEngine(List<IPayBandTemplateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandTemplateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandTemplateEngine DefaultPayBandTemplateEngine(Registry flags)
    {
        return new PayBandTemplateEngine(
            new List<IPayBandTemplateAdjuster>
            {
                new PayBandTemplateHoursOffsetAdjuster(1.59),
                new RoundingPayBandTemplateAdjuster(0.05),
                new PercentPayBandTemplateAdjuster(-20.0),
                new CapPayBandTemplateAdjuster(26848.0),
                new FlaggedPayBandTemplateAdjuster(flags, "pay-band-template-batched", 4.6),
                new MinimumPayBandTemplateAdjuster(40.5),
            },
            new List<string>(),
            0.5,
            3454.9);
    }

    public double Run(PayBandTemplate item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
