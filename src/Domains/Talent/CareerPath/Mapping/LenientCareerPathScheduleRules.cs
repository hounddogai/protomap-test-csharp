namespace Acme.Workforce.Talent.CareerPath.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient career path schedule must pass. Check returns its problems, or none.</summary>
public interface ILenientCareerPathScheduleRule
{
    List<string> Check(LenientCareerPathSchedule item);
}

public sealed record LenientCareerPathScheduleCodeLengthRule(int MaxLength) : ILenientCareerPathScheduleRule
{
    public List<string> Check(LenientCareerPathSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientCareerPathSchedulePriorityRangeRule(int Min, int Max) : ILenientCareerPathScheduleRule
{
    public List<string> Check(LenientCareerPathSchedule item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientCareerPathScheduleRequiredOwnerIdRule(string Message) : ILenientCareerPathScheduleRule
{
    public List<string> Check(LenientCareerPathSchedule item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientCareerPathScheduleBadgeLengthRule(int MaxLength) : ILenientCareerPathScheduleRule
{
    public List<string> Check(LenientCareerPathSchedule item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientCareerPathScheduleAllowedCodeRule(List<string> Allowed) : ILenientCareerPathScheduleRule
{
    public List<string> Check(LenientCareerPathSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient career path schedule.</summary>
public sealed record LenientCareerPathScheduleValidator(List<ILenientCareerPathScheduleRule> Rules)
{
    public List<string> Validate(LenientCareerPathSchedule item)
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

    public void RequireValid(LenientCareerPathSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientCareerPathSchedule> items)
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

/// <summary>Functions of lenient career path schedule rules.</summary>
public static class LenientCareerPathScheduleRules
{
    public static LenientCareerPathScheduleValidator DefaultLenientCareerPathScheduleValidator()
    {
        return new LenientCareerPathScheduleValidator(
            new List<ILenientCareerPathScheduleRule>
            {
                new LenientCareerPathScheduleCodeLengthRule(120),
                new LenientCareerPathSchedulePriorityRangeRule(64, 86400),
                new LenientCareerPathScheduleRequiredOwnerIdRule("lenient career path schedule needs this field"),
                new LenientCareerPathScheduleBadgeLengthRule(80),
                new LenientCareerPathScheduleAllowedCodeRule(new List<string> { "manual", "fallback", "regional" }),
            });
    }
}
