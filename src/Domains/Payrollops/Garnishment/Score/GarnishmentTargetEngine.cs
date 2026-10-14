namespace Acme.Workforce.Payrollops.Garnishment.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the garnishment target calculation.</summary>
public interface IGarnishmentTargetFactor
{
    string Describe();
    double Adjust(GarnishmentTarget item, double valueValue);
}

public sealed record GarnishmentTargetCreatedAtOffsetAdjuster(double Weight) : IGarnishmentTargetFactor
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(GarnishmentTarget item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record FlaggedGarnishmentTargetAdjuster(Registry Flags, string Flag, double Percent) : IGarnishmentTargetFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GarnishmentTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapGarnishmentTargetAdjuster(double Cap) : IGarnishmentTargetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CountryGarnishmentTargetAdjuster(Dictionary<string, double> Factors) : IGarnishmentTargetFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(GarnishmentTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingGarnishmentTargetAdjuster(double Step) : IGarnishmentTargetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GarnishmentTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the garnishment target steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentTargetEngine(List<IGarnishmentTargetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentTargetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentTargetEngine DefaultGarnishmentTargetEngine(Registry flags)
    {
        return new GarnishmentTargetEngine(
            new List<IGarnishmentTargetFactor>
            {
                new GarnishmentTargetCreatedAtOffsetAdjuster(1.96),
                new FlaggedGarnishmentTargetAdjuster(flags, "garnishment-target-adaptive", -5.3),
                new CapGarnishmentTargetAdjuster(15506.0),
                new CountryGarnishmentTargetAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.97, ["east"] = 1.22, ["west"] = 1.11 }),
                new RoundingGarnishmentTargetAdjuster(1.0),
            },
            new List<string>(),
            0.1,
            15737.1);
    }

    public double Evaluate(GarnishmentTarget item)
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
