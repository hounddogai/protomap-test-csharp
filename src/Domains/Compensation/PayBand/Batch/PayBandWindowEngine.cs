namespace Acme.Workforce.Compensation.PayBand.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band window calculation.</summary>
public interface IPayBandWindowFactor
{
    string Describe();
    double Adjust(PayBandWindow item, double valueValue);
}

public sealed record PercentPayBandWindowAdjuster(double Percent) : IPayBandWindowFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPayBandWindowAdjuster(double Cap) : IPayBandWindowFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPayBandWindowAdjuster(double Step) : IPayBandWindowFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitPayBandWindowAdjuster(Dictionary<string, double> Factors) : IPayBandWindowFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(PayBandWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PayBandWindowScoreOffsetAdjuster(double Weight) : IPayBandWindowFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(PayBandWindow item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record MinimumPayBandWindowAdjuster(double Minimum) : IPayBandWindowFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayBandWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pay band window steps in order and keeps a trace of each.</summary>
public sealed class PayBandWindowEngine(List<IPayBandWindowFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandWindowFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandWindowEngine DefaultPayBandWindowEngine()
    {
        return new PayBandWindowEngine(
            new List<IPayBandWindowFactor>
            {
                new PercentPayBandWindowAdjuster(-11.0),
                new CapPayBandWindowAdjuster(17070.0),
                new RoundingPayBandWindowAdjuster(0.1),
                new UnitPayBandWindowAdjuster(new Dictionary<string, double> { ["north"] = 1.17, ["retail"] = 0.85 }),
                new PayBandWindowScoreOffsetAdjuster(0.42),
                new MinimumPayBandWindowAdjuster(12.5),
            },
            new List<string>(),
            3.0,
            90207.7);
    }

    public double Price(PayBandWindow item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
