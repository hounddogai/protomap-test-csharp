namespace Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>One step of the default license seat profile calculation.</summary>
public interface IDefaultLicenseSeatProfileAdjuster
{
    string Describe();
    double Adjust(DefaultLicenseSeatProfile item, double valueValue);
}

public sealed record SourceDefaultLicenseSeatProfileAdjuster(Dictionary<string, double> Factors) : IDefaultLicenseSeatProfileAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(DefaultLicenseSeatProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumDefaultLicenseSeatProfileAdjuster(double Minimum) : IDefaultLicenseSeatProfileAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultLicenseSeatProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DefaultLicenseSeatProfileRateOffsetAdjuster(double Weight) : IDefaultLicenseSeatProfileAdjuster
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(DefaultLicenseSeatProfile item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the default license seat profile steps in order and keeps a trace of each.</summary>
public sealed class DefaultLicenseSeatProfileEngine(List<IDefaultLicenseSeatProfileAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultLicenseSeatProfileAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultLicenseSeatProfileEngine DefaultDefaultLicenseSeatProfileEngine()
    {
        return new DefaultLicenseSeatProfileEngine(
            new List<IDefaultLicenseSeatProfileAdjuster>
            {
                new SourceDefaultLicenseSeatProfileAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.14, ["retail"] = 0.73 }),
                new MinimumDefaultLicenseSeatProfileAdjuster(39.9),
                new DefaultLicenseSeatProfileRateOffsetAdjuster(0.51),
            },
            new List<string>(),
            4.2,
            10218.7);
    }

    public double Evaluate(DefaultLicenseSeatProfile item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultLicenseSeatProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
