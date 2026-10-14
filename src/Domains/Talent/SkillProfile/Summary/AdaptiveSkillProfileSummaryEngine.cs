namespace Acme.Workforce.Talent.SkillProfile.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive skill profile summary calculation.</summary>
public interface IAdaptiveSkillProfileSummaryModifier
{
    string Describe();
    double Adjust(AdaptiveSkillProfileSummary item, double valueValue);
}

public sealed record RoundingAdaptiveSkillProfileSummaryAdjuster(double Step) : IAdaptiveSkillProfileSummaryModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveSkillProfileSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAdaptiveSkillProfileSummaryAdjuster(double Percent) : IAdaptiveSkillProfileSummaryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveSkillProfileSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AdaptiveSkillProfileSummaryWindowDaysOffsetAdjuster(double Weight) : IAdaptiveSkillProfileSummaryModifier
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(AdaptiveSkillProfileSummary item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record NameAdaptiveSkillProfileSummaryAdjuster(Dictionary<string, double> Factors) : IAdaptiveSkillProfileSummaryModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AdaptiveSkillProfileSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adaptive skill profile summary steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveSkillProfileSummaryEngine(List<IAdaptiveSkillProfileSummaryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveSkillProfileSummaryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveSkillProfileSummaryEngine DefaultAdaptiveSkillProfileSummaryEngine()
    {
        return new AdaptiveSkillProfileSummaryEngine(
            new List<IAdaptiveSkillProfileSummaryModifier>
            {
                new RoundingAdaptiveSkillProfileSummaryAdjuster(0.5),
                new PercentAdaptiveSkillProfileSummaryAdjuster(-7.8),
                new AdaptiveSkillProfileSummaryWindowDaysOffsetAdjuster(0.71),
                new NameAdaptiveSkillProfileSummaryAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.07, ["retail"] = 1.22 }),
            },
            new List<string>(),
            2.5,
            40201.4);
    }

    public double Price(AdaptiveSkillProfileSummary item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveSkillProfileSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
