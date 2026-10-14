namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the laptop profile calculation.</summary>
public interface ILaptopProfileFactor
{
    string Describe();
    double Adjust(LaptopProfile item, double valueValue);
}

public sealed record FlaggedLaptopProfileAdjuster(Registry Flags, string Flag, double Percent) : ILaptopProfileFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LaptopProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLaptopProfileAdjuster(double Cap) : ILaptopProfileFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLaptopProfileAdjuster(double Minimum) : ILaptopProfileFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLaptopProfileAdjuster(double Percent) : ILaptopProfileFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the laptop profile steps in order and keeps a trace of each.</summary>
public sealed class LaptopProfileEngine(List<ILaptopProfileFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopProfileFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopProfileEngine DefaultLaptopProfileEngine(Registry flags)
    {
        return new LaptopProfileEngine(
            new List<ILaptopProfileFactor>
            {
                new FlaggedLaptopProfileAdjuster(flags, "laptop-profile-batched", -8.2),
                new CapLaptopProfileAdjuster(17227.0),
                new MinimumLaptopProfileAdjuster(9.8),
                new PercentLaptopProfileAdjuster(-19.2),
            },
            new List<string>(),
            1.2,
            54272.2);
    }

    public double Evaluate(LaptopProfile item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
