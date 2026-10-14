namespace Acme.Workforce.Talent.Mobility.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility template calculation.</summary>
public interface IMobilityTemplateModifier
{
    string Describe();
    double Adjust(MobilityTemplate item, double valueValue);
}

public sealed record MobilityTemplateCreatedAtOffsetAdjuster(double Weight) : IMobilityTemplateModifier
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(MobilityTemplate item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record CapMobilityTemplateAdjuster(double Cap) : IMobilityTemplateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilityTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumMobilityTemplateAdjuster(double Minimum) : IMobilityTemplateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MobilityTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedMobilityTemplateAdjuster(Registry Flags, string Flag, double Percent) : IMobilityTemplateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MobilityTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelMobilityTemplateAdjuster(Dictionary<string, double> Factors) : IMobilityTemplateModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(MobilityTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the mobility template steps in order and keeps a trace of each.</summary>
public sealed class MobilityTemplateEngine(List<IMobilityTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityTemplateEngine DefaultMobilityTemplateEngine(Registry flags)
    {
        return new MobilityTemplateEngine(
            new List<IMobilityTemplateModifier>
            {
                new MobilityTemplateCreatedAtOffsetAdjuster(0.78),
                new CapMobilityTemplateAdjuster(40181.0),
                new MinimumMobilityTemplateAdjuster(44.1),
                new FlaggedMobilityTemplateAdjuster(flags, "mobility-template-primary", -3.2),
                new LabelMobilityTemplateAdjuster(new Dictionary<string, double> { ["north"] = 0.99 }),
            },
            new List<string>(),
            2.7,
            44762.2);
    }

    public double Compute(MobilityTemplate item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
