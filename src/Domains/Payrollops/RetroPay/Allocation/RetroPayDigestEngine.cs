namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>One step of the retro pay digest calculation.</summary>
public interface IRetroPayDigestModifier
{
    string Describe();
    double Adjust(RetroPayDigest item, double valueValue);
}

public sealed record CategoryRetroPayDigestAdjuster(Dictionary<string, double> Factors) : IRetroPayDigestModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(RetroPayDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredRetroPayDigestAdjuster(List<double> Bounds, List<double> Rates) : IRetroPayDigestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RetroPayDigest item, double valueValue)
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

public sealed record CapRetroPayDigestAdjuster(double Cap) : IRetroPayDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RetroPayDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the retro pay digest steps in order and keeps a trace of each.</summary>
public sealed class RetroPayDigestEngine(List<IRetroPayDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayDigestEngine DefaultRetroPayDigestEngine()
    {
        return new RetroPayDigestEngine(
            new List<IRetroPayDigestModifier>
            {
                new CategoryRetroPayDigestAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.78, ["south"] = 1.32, ["retail"] = 1.1 }),
                new TieredRetroPayDigestAdjuster(
                    new List<double> { 1061.0, 2489.0, 4597.0, 5747.0 },
                    new List<double> { 1.02, 0.99, 0.85, 1.01 }),
                new CapRetroPayDigestAdjuster(24466.0),
            },
            new List<string>(),
            0.0,
            72336.0);
    }

    public double Run(RetroPayDigest item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
