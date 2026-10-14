namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

/// <summary>A rule a global equity grant schedule must pass. Check returns its problems, or none.</summary>
public interface IGlobalEquityGrantScheduleRule
{
    List<string> Check(GlobalEquityGrantSchedule item);
}

public sealed record GlobalEquityGrantScheduleRequiredNameRule(string Message) : IGlobalEquityGrantScheduleRule
{
    public List<string> Check(GlobalEquityGrantSchedule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalEquityGrantScheduleSkillLengthRule(int MaxLength) : IGlobalEquityGrantScheduleRule
{
    public List<string> Check(GlobalEquityGrantSchedule item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalEquityGrantScheduleExpiresAtRangeRule(int Min, int Max) : IGlobalEquityGrantScheduleRule
{
    public List<string> Check(GlobalEquityGrantSchedule item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalEquityGrantScheduleRequiredSkillRule(string Message) : IGlobalEquityGrantScheduleRule
{
    public List<string> Check(GlobalEquityGrantSchedule item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global equity grant schedule.</summary>
public sealed record GlobalEquityGrantScheduleValidator(List<IGlobalEquityGrantScheduleRule> Rules)
{
    public List<string> Validate(GlobalEquityGrantSchedule item)
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

    public void RequireValid(GlobalEquityGrantSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalEquityGrantSchedule> items)
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

/// <summary>Functions of global equity grant schedule rules.</summary>
public static class GlobalEquityGrantScheduleRules
{
    public static GlobalEquityGrantScheduleValidator DefaultGlobalEquityGrantScheduleValidator()
    {
        return new GlobalEquityGrantScheduleValidator(
            new List<IGlobalEquityGrantScheduleRule>
            {
                new GlobalEquityGrantScheduleRequiredNameRule("global equity grant schedule needs this field"),
                new GlobalEquityGrantScheduleSkillLengthRule(255),
                new GlobalEquityGrantScheduleExpiresAtRangeRule(120, 255),
                new GlobalEquityGrantScheduleRequiredSkillRule("global equity grant schedule needs this field"),
            });
    }
}
