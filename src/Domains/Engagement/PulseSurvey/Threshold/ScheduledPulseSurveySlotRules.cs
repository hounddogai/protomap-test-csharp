namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>A rule a scheduled pulse survey slot must pass. Check returns its problems, or none.</summary>
public interface IScheduledPulseSurveySlotRule
{
    List<string> Check(ScheduledPulseSurveySlot item);
}

public sealed record ScheduledPulseSurveySlotMaximumRangeRule(double Min, double Max) : IScheduledPulseSurveySlotRule
{
    public List<string> Check(ScheduledPulseSurveySlot item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledPulseSurveySlotCodePrefixRule(string Prefix) : IScheduledPulseSurveySlotRule
{
    public List<string> Check(ScheduledPulseSurveySlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPulseSurveySlotRequiredTitleRule(string Message) : IScheduledPulseSurveySlotRule
{
    public List<string> Check(ScheduledPulseSurveySlot item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPulseSurveySlotNameLengthRule(int MaxLength) : IScheduledPulseSurveySlotRule
{
    public List<string> Check(ScheduledPulseSurveySlot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled pulse survey slot.</summary>
public sealed record ScheduledPulseSurveySlotValidator(List<IScheduledPulseSurveySlotRule> Rules)
{
    public List<string> Validate(ScheduledPulseSurveySlot item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(ScheduledPulseSurveySlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPulseSurveySlot> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of scheduled pulse survey slot rules.</summary>
public static class ScheduledPulseSurveySlotRules
{
    public static ScheduledPulseSurveySlotValidator DefaultScheduledPulseSurveySlotValidator()
    {
        return new ScheduledPulseSurveySlotValidator(
            new List<IScheduledPulseSurveySlotRule>
            {
                new ScheduledPulseSurveySlotMaximumRangeRule(8.3, 1366.9),
                new ScheduledPulseSurveySlotCodePrefixRule("OPS"),
                new ScheduledPulseSurveySlotRequiredTitleRule("scheduled pulse survey slot needs this field"),
                new ScheduledPulseSurveySlotNameLengthRule(1000),
            });
    }
}
