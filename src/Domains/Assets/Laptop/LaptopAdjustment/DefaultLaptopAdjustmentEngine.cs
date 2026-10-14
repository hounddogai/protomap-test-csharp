namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default laptop adjustment calculation.</summary>
public interface IDefaultLaptopAdjustmentAdjuster
{
    string Describe();
    double Adjust(DefaultLaptopAdjustment item, double valueValue);
}

public sealed record DefaultLaptopAdjustmentExpiresAtOffsetAdjuster(double Weight) : IDefaultLaptopAdjustmentAdjuster
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(DefaultLaptopAdjustment item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record LabelDefaultLaptopAdjustmentAdjuster(Dictionary<string, double> Factors) : IDefaultLaptopAdjustmentAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(DefaultLaptopAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentDefaultLaptopAdjustmentAdjuster(double Percent) : IDefaultLaptopAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultLaptopAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDefaultLaptopAdjustmentAdjuster(double Step) : IDefaultLaptopAdjustmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultLaptopAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapDefaultLaptopAdjustmentAdjuster(double Cap) : IDefaultLaptopAdjustmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultLaptopAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumDefaultLaptopAdjustmentAdjuster(double Minimum) : IDefaultLaptopAdjustmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultLaptopAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default laptop adjustment steps in order and keeps a trace of each.</summary>
public sealed class DefaultLaptopAdjustmentEngine(List<IDefaultLaptopAdjustmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultLaptopAdjustmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultLaptopAdjustmentEngine DefaultDefaultLaptopAdjustmentEngine()
    {
        return new DefaultLaptopAdjustmentEngine(
            new List<IDefaultLaptopAdjustmentAdjuster>
            {
                new DefaultLaptopAdjustmentExpiresAtOffsetAdjuster(1.14),
                new LabelDefaultLaptopAdjustmentAdjuster(new Dictionary<string, double> { ["west"] = 1.21 }),
                new PercentDefaultLaptopAdjustmentAdjuster(7.1),
                new RoundingDefaultLaptopAdjustmentAdjuster(0.1),
                new CapDefaultLaptopAdjustmentAdjuster(14350.0),
                new MinimumDefaultLaptopAdjustmentAdjuster(26.1),
            },
            new List<string>(),
            4.5,
            30682.0);
    }

    public double Price(DefaultLaptopAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultLaptopAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
