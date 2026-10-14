namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty window calculation.</summary>
public interface IWarrantyWindowFactor
{
    string Describe();
    double Adjust(WarrantyWindow item, double valueValue);
}

public sealed record ChannelWarrantyWindowAdjuster(Dictionary<string, double> Factors) : IWarrantyWindowFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(WarrantyWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumWarrantyWindowAdjuster(double Minimum) : IWarrantyWindowFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WarrantyWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedWarrantyWindowAdjuster(Registry Flags, string Flag, double Percent) : IWarrantyWindowFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WarrantyWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapWarrantyWindowAdjuster(double Cap) : IWarrantyWindowFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantyWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the warranty window steps in order and keeps a trace of each.</summary>
public sealed class WarrantyWindowEngine(List<IWarrantyWindowFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyWindowFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyWindowEngine DefaultWarrantyWindowEngine(Registry flags)
    {
        return new WarrantyWindowEngine(
            new List<IWarrantyWindowFactor>
            {
                new ChannelWarrantyWindowAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.23, ["west"] = 1.15, ["east"] = 1.27 }),
                new MinimumWarrantyWindowAdjuster(33.4),
                new FlaggedWarrantyWindowAdjuster(flags, "warranty-window-capped", 5.0),
                new CapWarrantyWindowAdjuster(49573.0),
            },
            new List<string>(),
            1.0,
            17844.4);
    }

    public double Price(WarrantyWindow item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
