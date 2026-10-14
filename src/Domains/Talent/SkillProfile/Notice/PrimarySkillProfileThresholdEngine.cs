namespace Acme.Workforce.Talent.SkillProfile.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary skill profile threshold calculation.</summary>
public interface IPrimarySkillProfileThresholdAdjuster
{
    string Describe();
    double Adjust(PrimarySkillProfileThreshold item, double valueValue);
}

public sealed record FlaggedPrimarySkillProfileThresholdAdjuster(Registry Flags, string Flag, double Percent) : IPrimarySkillProfileThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimarySkillProfileThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPrimarySkillProfileThresholdAdjuster(List<double> Bounds, List<double> Rates) : IPrimarySkillProfileThresholdAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimarySkillProfileThreshold item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record RoundingPrimarySkillProfileThresholdAdjuster(double Step) : IPrimarySkillProfileThresholdAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimarySkillProfileThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the primary skill profile threshold steps in order and keeps a trace of each.</summary>
public sealed class PrimarySkillProfileThresholdEngine(List<IPrimarySkillProfileThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimarySkillProfileThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimarySkillProfileThresholdEngine DefaultPrimarySkillProfileThresholdEngine(Registry flags)
    {
        return new PrimarySkillProfileThresholdEngine(
            new List<IPrimarySkillProfileThresholdAdjuster>
            {
                new FlaggedPrimarySkillProfileThresholdAdjuster(
                    flags,
                    "primary-skill-profile-threshold-manual",
                    -0.7),
                new TieredPrimarySkillProfileThresholdAdjuster(
                    new List<double> { 126.0, 1331.0, 4019.0, 9873.0 },
                    new List<double> { 0.85, 0.81, 1.14, 0.86 }),
                new RoundingPrimarySkillProfileThresholdAdjuster(0.05),
            },
            new List<string>(),
            1.7,
            83240.4);
    }

    public double Evaluate(PrimarySkillProfileThreshold item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PrimarySkillProfileThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
