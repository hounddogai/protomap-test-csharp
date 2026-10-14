namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey schedule must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyScheduleRule
{
    List<string> Check(PulseSurveySchedule item);
}

public sealed record PulseSurveyScheduleSequenceRangeRule(int Min, int Max) : IPulseSurveyScheduleRule
{
    public List<string> Check(PulseSurveySchedule item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyScheduleRequiredCodeRule(string Message) : IPulseSurveyScheduleRule
{
    public List<string> Check(PulseSurveySchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyScheduleStartsAtRangeRule(int Min, int Max) : IPulseSurveyScheduleRule
{
    public List<string> Check(PulseSurveySchedule item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PulseSurveyScheduleAllowedNameRule(List<string> Allowed) : IPulseSurveyScheduleRule
{
    public List<string> Check(PulseSurveySchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyScheduleEmployeeIdLengthRule(int MaxLength) : IPulseSurveyScheduleRule
{
    public List<string> Check(PulseSurveySchedule item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey schedule.</summary>
public sealed record PulseSurveyScheduleValidator(List<IPulseSurveyScheduleRule> Rules)
{
    public List<string> Validate(PulseSurveySchedule item)
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

    public void RequireValid(PulseSurveySchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveySchedule> items)
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

/// <summary>Functions of pulse survey schedule rules.</summary>
public static class PulseSurveyScheduleRules
{
    public static PulseSurveyScheduleValidator DefaultPulseSurveyScheduleValidator()
    {
        return new PulseSurveyScheduleValidator(
            new List<IPulseSurveyScheduleRule>
            {
                new PulseSurveyScheduleSequenceRangeRule(32, 86400),
                new PulseSurveyScheduleRequiredCodeRule("pulse survey schedule needs this field"),
                new PulseSurveyScheduleStartsAtRangeRule(255, 1000),
                new PulseSurveyScheduleAllowedNameRule(new List<string> { "fallback" }),
                new PulseSurveyScheduleEmployeeIdLengthRule(64),
            });
    }
}
