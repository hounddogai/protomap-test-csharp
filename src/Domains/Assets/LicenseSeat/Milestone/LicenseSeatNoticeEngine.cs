namespace Acme.Workforce.Assets.LicenseSeat.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat notice calculation.</summary>
public interface ILicenseSeatNoticeAdjuster
{
    string Describe();
    double Adjust(LicenseSeatNotice item, double valueValue);
}

public sealed record CategoryLicenseSeatNoticeAdjuster(Dictionary<string, double> Factors) : ILicenseSeatNoticeAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(LicenseSeatNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingLicenseSeatNoticeAdjuster(double Step) : ILicenseSeatNoticeAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LicenseSeatNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLicenseSeatNoticeAdjuster(double Cap) : ILicenseSeatNoticeAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLicenseSeatNoticeAdjuster(double Percent) : ILicenseSeatNoticeAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LicenseSeatNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the license seat notice steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatNoticeEngine(List<ILicenseSeatNoticeAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatNoticeAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatNoticeEngine DefaultLicenseSeatNoticeEngine()
    {
        return new LicenseSeatNoticeEngine(
            new List<ILicenseSeatNoticeAdjuster>
            {
                new CategoryLicenseSeatNoticeAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.17, ["west"] = 1.37 }),
                new RoundingLicenseSeatNoticeAdjuster(0.1),
                new CapLicenseSeatNoticeAdjuster(6022.0),
                new PercentLicenseSeatNoticeAdjuster(-12.7),
            },
            new List<string>(),
            2.1,
            72448.6);
    }

    public double Run(LicenseSeatNotice item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
