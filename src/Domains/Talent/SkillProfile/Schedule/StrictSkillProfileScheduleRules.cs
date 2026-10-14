namespace Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>A rule a strict skill profile schedule must pass. Check returns its problems, or none.</summary>
public interface IStrictSkillProfileScheduleRule
{
    List<string> Check(StrictSkillProfileSchedule item);
}

public sealed record StrictSkillProfileScheduleVisibleGuardRule(double Limit) : IStrictSkillProfileScheduleRule
{
    public List<string> Check(StrictSkillProfileSchedule item)
    {
        if (item.Visible && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record StrictSkillProfileScheduleRequiredCodeRule(string Message) : IStrictSkillProfileScheduleRule
{
    public List<string> Check(StrictSkillProfileSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictSkillProfileScheduleRequiredNotesRule(string Message) : IStrictSkillProfileScheduleRule
{
    public List<string> Check(StrictSkillProfileSchedule item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictSkillProfileScheduleNotesLengthRule(int MaxLength) : IStrictSkillProfileScheduleRule
{
    public List<string> Check(StrictSkillProfileSchedule item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictSkillProfileScheduleFteRangeRule(double Min, double Max) : IStrictSkillProfileScheduleRule
{
    public List<string> Check(StrictSkillProfileSchedule item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictSkillProfileScheduleRequiredNameRule(string Message) : IStrictSkillProfileScheduleRule
{
    public List<string> Check(StrictSkillProfileSchedule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict skill profile schedule.</summary>
public sealed record StrictSkillProfileScheduleValidator(List<IStrictSkillProfileScheduleRule> Rules)
{
    public List<string> Validate(StrictSkillProfileSchedule item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(StrictSkillProfileSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictSkillProfileSchedule> items)
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

/// <summary>Functions of strict skill profile schedule rules.</summary>
public static class StrictSkillProfileScheduleRules
{
    public static StrictSkillProfileScheduleValidator DefaultStrictSkillProfileScheduleValidator()
    {
        return new StrictSkillProfileScheduleValidator(
            new List<IStrictSkillProfileScheduleRule>
            {
                new StrictSkillProfileScheduleVisibleGuardRule(7.1),
                new StrictSkillProfileScheduleRequiredCodeRule("strict skill profile schedule needs a value"),
                new StrictSkillProfileScheduleRequiredNotesRule("strict skill profile schedule needs this field"),
                new StrictSkillProfileScheduleNotesLengthRule(16),
                new StrictSkillProfileScheduleFteRangeRule(6.3, 295.2),
                new StrictSkillProfileScheduleRequiredNameRule("strict skill profile schedule needs this field"),
            });
    }
}
