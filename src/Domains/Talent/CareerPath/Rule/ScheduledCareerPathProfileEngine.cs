namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled career path profile calculation.</summary>
public interface IScheduledCareerPathProfileStep
{
    string Describe();
    double Adjust(ScheduledCareerPathProfile item, double valueValue);
}

public sealed record ScheduledCareerPathProfileUpdatedAtOffsetAdjuster(double Weight) : IScheduledCareerPathProfileStep
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(ScheduledCareerPathProfile item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record CountryScheduledCareerPathProfileAdjuster(Dictionary<string, double> Factors) : IScheduledCareerPathProfileStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(ScheduledCareerPathProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedScheduledCareerPathProfileAdjuster(Registry Flags, string Flag, double Percent) : IScheduledCareerPathProfileStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledCareerPathProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled career path profile steps in order and keeps a trace of each.</summary>
public sealed class ScheduledCareerPathProfileEngine(List<IScheduledCareerPathProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledCareerPathProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledCareerPathProfileEngine DefaultScheduledCareerPathProfileEngine(Registry flags)
    {
        return new ScheduledCareerPathProfileEngine(
            new List<IScheduledCareerPathProfileStep>
            {
                new ScheduledCareerPathProfileUpdatedAtOffsetAdjuster(0.57),
                new CountryScheduledCareerPathProfileAdjuster(new Dictionary<string, double> { ["central"] = 1.32 }),
                new FlaggedScheduledCareerPathProfileAdjuster(flags, "scheduled-career-path-profile-adaptive", -8.0),
            },
            new List<string>(),
            2.6,
            12571.2);
    }

    public double Run(ScheduledCareerPathProfile item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledCareerPathProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
