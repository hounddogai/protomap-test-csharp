namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a holiday assignment must pass. Check returns its problems, or none.</summary>
public interface IHolidayAssignmentRule
{
    List<string> Check(HolidayAssignment item);
}

public sealed record HolidayAssignmentSkillLengthRule(int MaxLength) : IHolidayAssignmentRule
{
    public List<string> Check(HolidayAssignment item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayAssignmentCodeLengthRule(int MaxLength) : IHolidayAssignmentRule
{
    public List<string> Check(HolidayAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayAssignmentRequiredSkillRule(string Message) : IHolidayAssignmentRule
{
    public List<string> Check(HolidayAssignment item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayAssignmentAllowedCodeRule(List<string> Allowed) : IHolidayAssignmentRule
{
    public List<string> Check(HolidayAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record HolidayAssignmentTimezoneLengthRule(int MaxLength) : IHolidayAssignmentRule
{
    public List<string> Check(HolidayAssignment item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayAssignmentRequiredTimezoneRule(string Message) : IHolidayAssignmentRule
{
    public List<string> Check(HolidayAssignment item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday assignment.</summary>
public sealed record HolidayAssignmentValidator(List<IHolidayAssignmentRule> Rules)
{
    public List<string> Validate(HolidayAssignment item)
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

    public void RequireValid(HolidayAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayAssignment> items)
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

/// <summary>Functions of holiday assignment rules.</summary>
public static class HolidayAssignmentRules
{
    public static HolidayAssignmentValidator DefaultHolidayAssignmentValidator()
    {
        return new HolidayAssignmentValidator(
            new List<IHolidayAssignmentRule>
            {
                new HolidayAssignmentSkillLengthRule(1000),
                new HolidayAssignmentCodeLengthRule(120),
                new HolidayAssignmentRequiredSkillRule("holiday assignment needs an entry"),
                new HolidayAssignmentAllowedCodeRule(new List<string> { "manual", "tiered" }),
                new HolidayAssignmentTimezoneLengthRule(255),
                new HolidayAssignmentRequiredTimezoneRule("holiday assignment needs an entry"),
            });
    }
}
