namespace Acme.Workforce.Payrollops.Garnishment.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the garnishment profile calculation.</summary>
public interface IGarnishmentProfileAdjuster
{
    string Describe();
    double Adjust(GarnishmentProfile item, double valueValue);
}

public sealed record FlaggedGarnishmentProfileAdjuster(Registry Flags, string Flag, double Percent) : IGarnishmentProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GarnishmentProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleGarnishmentProfileAdjuster(Dictionary<string, double> Factors) : IGarnishmentProfileAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(GarnishmentProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingGarnishmentProfileAdjuster(double Step) : IGarnishmentProfileAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GarnishmentProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentGarnishmentProfileAdjuster(double Percent) : IGarnishmentProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GarnishmentProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the garnishment profile steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentProfileEngine(List<IGarnishmentProfileAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentProfileAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentProfileEngine DefaultGarnishmentProfileEngine(Registry flags)
    {
        return new GarnishmentProfileEngine(
            new List<IGarnishmentProfileAdjuster>
            {
                new FlaggedGarnishmentProfileAdjuster(flags, "garnishment-profile-adaptive", 7.5),
                new TitleGarnishmentProfileAdjuster(new Dictionary<string, double> { ["online"] = 1.15 }),
                new RoundingGarnishmentProfileAdjuster(5.0),
                new PercentGarnishmentProfileAdjuster(-18.7),
            },
            new List<string>(),
            5.0,
            71883.3);
    }

    public double Compute(GarnishmentProfile item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
