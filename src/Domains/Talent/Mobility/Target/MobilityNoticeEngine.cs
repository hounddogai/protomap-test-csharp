namespace Acme.Workforce.Talent.Mobility.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility notice calculation.</summary>
public interface IMobilityNoticeModifier
{
    string Describe();
    double Adjust(MobilityNotice item, double valueValue);
}

public sealed record MobilityNoticeRateOffsetAdjuster(double Weight) : IMobilityNoticeModifier
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(MobilityNotice item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record MinimumMobilityNoticeAdjuster(double Minimum) : IMobilityNoticeModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MobilityNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapMobilityNoticeAdjuster(double Cap) : IMobilityNoticeModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilityNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentMobilityNoticeAdjuster(double Percent) : IMobilityNoticeModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MobilityNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the mobility notice steps in order and keeps a trace of each.</summary>
public sealed class MobilityNoticeEngine(List<IMobilityNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityNoticeEngine DefaultMobilityNoticeEngine()
    {
        return new MobilityNoticeEngine(
            new List<IMobilityNoticeModifier>
            {
                new MobilityNoticeRateOffsetAdjuster(1.15),
                new MinimumMobilityNoticeAdjuster(21.7),
                new CapMobilityNoticeAdjuster(16355.0),
                new PercentMobilityNoticeAdjuster(15.3),
            },
            new List<string>(),
            4.1,
            79361.0);
    }

    public double Evaluate(MobilityNotice item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
