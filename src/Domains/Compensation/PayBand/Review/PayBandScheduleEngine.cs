namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band schedule calculation.</summary>
public interface IPayBandScheduleModifier
{
    string Describe();
    double Adjust(PayBandSchedule item, double valueValue);
}

public sealed record CapPayBandScheduleAdjuster(double Cap) : IPayBandScheduleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DepartmentPayBandScheduleAdjuster(Dictionary<string, double> Factors) : IPayBandScheduleModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PayBandSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPayBandScheduleAdjuster(double Step) : IPayBandScheduleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPayBandScheduleAdjuster(double Percent) : IPayBandScheduleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPayBandScheduleAdjuster(double Minimum) : IPayBandScheduleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayBandSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPayBandScheduleAdjuster(List<double> Bounds, List<double> Rates) : IPayBandScheduleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandSchedule item, double valueValue)
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

/// <summary>Runs the pay band schedule steps in order and keeps a trace of each.</summary>
public sealed class PayBandScheduleEngine(List<IPayBandScheduleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandScheduleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandScheduleEngine DefaultPayBandScheduleEngine()
    {
        return new PayBandScheduleEngine(
            new List<IPayBandScheduleModifier>
            {
                new CapPayBandScheduleAdjuster(9285.0),
                new DepartmentPayBandScheduleAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.88, ["south"] = 1.3, ["central"] = 0.9 }),
                new RoundingPayBandScheduleAdjuster(0.1),
                new PercentPayBandScheduleAdjuster(0.7),
                new MinimumPayBandScheduleAdjuster(20.7),
                new TieredPayBandScheduleAdjuster(
                    new List<double> { 308.0, 2947.0, 5513.0 },
                    new List<double> { 1.01, 0.93, 1.02 }),
            },
            new List<string>(),
            0.7,
            59465.3);
    }

    public double Price(PayBandSchedule item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
