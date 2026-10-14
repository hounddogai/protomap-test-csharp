namespace Acme.Workforce.Talent.SkillProfile.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy skill profile checkpoint calculation.</summary>
public interface ILegacySkillProfileCheckpointModifier
{
    string Describe();
    double Adjust(LegacySkillProfileCheckpoint item, double valueValue);
}

public sealed record PercentLegacySkillProfileCheckpointAdjuster(double Percent) : ILegacySkillProfileCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacySkillProfileCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameLegacySkillProfileCheckpointAdjuster(Dictionary<string, double> Factors) : ILegacySkillProfileCheckpointModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LegacySkillProfileCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LegacySkillProfileCheckpointHoursOffsetAdjuster(double Weight) : ILegacySkillProfileCheckpointModifier
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(LegacySkillProfileCheckpoint item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

/// <summary>Runs the legacy skill profile checkpoint steps in order and keeps a trace of each.</summary>
public sealed class LegacySkillProfileCheckpointEngine(List<ILegacySkillProfileCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacySkillProfileCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacySkillProfileCheckpointEngine DefaultLegacySkillProfileCheckpointEngine()
    {
        return new LegacySkillProfileCheckpointEngine(
            new List<ILegacySkillProfileCheckpointModifier>
            {
                new PercentLegacySkillProfileCheckpointAdjuster(18.9),
                new NameLegacySkillProfileCheckpointAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.04, ["central"] = 1.29, ["north"] = 1.07 }),
                new LegacySkillProfileCheckpointHoursOffsetAdjuster(0.42),
            },
            new List<string>(),
            2.8,
            26357.1);
    }

    public double Evaluate(LegacySkillProfileCheckpoint item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LegacySkillProfileCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
