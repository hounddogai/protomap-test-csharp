namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile record calculation.</summary>
public interface ISkillProfileRecordStep
{
    string Describe();
    double Adjust(SkillProfileRecord item, double valueValue);
}

public sealed record MinimumSkillProfileRecordAdjuster(double Minimum) : ISkillProfileRecordStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfileRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapSkillProfileRecordAdjuster(double Cap) : ISkillProfileRecordStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SkillProfileRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedSkillProfileRecordAdjuster(Registry Flags, string Flag, double Percent) : ISkillProfileRecordStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SkillProfileRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentSkillProfileRecordAdjuster(double Percent) : ISkillProfileRecordStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SkillProfileRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSkillProfileRecordAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileRecordStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileRecord item, double valueValue)
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

public sealed record SkillProfileRecordLeaveDaysOffsetAdjuster(double Weight) : ISkillProfileRecordStep
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(SkillProfileRecord item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

/// <summary>Runs the skill profile record steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileRecordEngine(List<ISkillProfileRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileRecordEngine DefaultSkillProfileRecordEngine(Registry flags)
    {
        return new SkillProfileRecordEngine(
            new List<ISkillProfileRecordStep>
            {
                new MinimumSkillProfileRecordAdjuster(42.5),
                new CapSkillProfileRecordAdjuster(17436.0),
                new FlaggedSkillProfileRecordAdjuster(flags, "skill-profile-record-weighted", 5.5),
                new PercentSkillProfileRecordAdjuster(8.7),
                new TieredSkillProfileRecordAdjuster(
                    new List<double> { 742.0, 1620.0, 4973.0, 7281.0 },
                    new List<double> { 0.87, 0.84, 0.9, 0.94 }),
                new SkillProfileRecordLeaveDaysOffsetAdjuster(1.92),
            },
            new List<string>(),
            1.1,
            8339.2);
    }

    public double Compute(SkillProfileRecord item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
