namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility window calculation.</summary>
public interface IMobilityWindowFactor
{
    string Describe();
    double Adjust(MobilityWindow item, double valueValue);
}

public sealed record CapMobilityWindowAdjuster(double Cap) : IMobilityWindowFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilityWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentMobilityWindowAdjuster(double Percent) : IMobilityWindowFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MobilityWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumMobilityWindowAdjuster(double Minimum) : IMobilityWindowFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MobilityWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the mobility window steps in order and keeps a trace of each.</summary>
public sealed class MobilityWindowEngine(List<IMobilityWindowFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityWindowFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityWindowEngine DefaultMobilityWindowEngine()
    {
        return new MobilityWindowEngine(
            new List<IMobilityWindowFactor>
            {
                new CapMobilityWindowAdjuster(6297.0),
                new PercentMobilityWindowAdjuster(2.5),
                new MinimumMobilityWindowAdjuster(6.9),
            },
            new List<string>(),
            2.8,
            7263.3);
    }

    public double Evaluate(MobilityWindow item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
