namespace Acme.Workforce.Compensation.EquityGrant.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant window calculation.</summary>
public interface IEquityGrantWindowAdjuster
{
    string Describe();
    double Adjust(EquityGrantWindow item, double valueValue);
}

public sealed record CapEquityGrantWindowAdjuster(double Cap) : IEquityGrantWindowAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record EquityGrantWindowLimitOffsetAdjuster(double Weight) : IEquityGrantWindowAdjuster
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(EquityGrantWindow item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record PercentEquityGrantWindowAdjuster(double Percent) : IEquityGrantWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EquityGrantWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumEquityGrantWindowAdjuster(double Minimum) : IEquityGrantWindowAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EquityGrantWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredEquityGrantWindowAdjuster(List<double> Bounds, List<double> Rates) : IEquityGrantWindowAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EquityGrantWindow item, double valueValue)
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

public sealed record RoundingEquityGrantWindowAdjuster(double Step) : IEquityGrantWindowAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EquityGrantWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the equity grant window steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantWindowEngine(List<IEquityGrantWindowAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantWindowAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantWindowEngine DefaultEquityGrantWindowEngine()
    {
        return new EquityGrantWindowEngine(
            new List<IEquityGrantWindowAdjuster>
            {
                new CapEquityGrantWindowAdjuster(26388.0),
                new EquityGrantWindowLimitOffsetAdjuster(0.06),
                new PercentEquityGrantWindowAdjuster(22.5),
                new MinimumEquityGrantWindowAdjuster(5.0),
                new TieredEquityGrantWindowAdjuster(
                    new List<double> { 842.0, 3764.0, 4131.0 },
                    new List<double> { 0.8, 0.87, 1.02 }),
                new RoundingEquityGrantWindowAdjuster(0.25),
            },
            new List<string>(),
            0.4,
            19075.0);
    }

    public double Compute(EquityGrantWindow item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
