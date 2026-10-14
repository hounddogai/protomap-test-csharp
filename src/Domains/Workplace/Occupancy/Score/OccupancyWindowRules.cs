namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy window must pass. Check returns its problems, or none.</summary>
public interface IOccupancyWindowRule
{
    List<string> Check(OccupancyWindow item);
}

public sealed record OccupancyWindowRequiredLabelRule(string Message) : IOccupancyWindowRule
{
    public List<string> Check(OccupancyWindow item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyWindowHoursRangeRule(double Min, double Max) : IOccupancyWindowRule
{
    public List<string> Check(OccupancyWindow item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyWindowAllowedCodeRule(List<string> Allowed) : IOccupancyWindowRule
{
    public List<string> Check(OccupancyWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyWindowSkillLengthRule(int MaxLength) : IOccupancyWindowRule
{
    public List<string> Check(OccupancyWindow item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyWindowLabelLengthRule(int MaxLength) : IOccupancyWindowRule
{
    public List<string> Check(OccupancyWindow item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyWindowRequiredCodeRule(string Message) : IOccupancyWindowRule
{
    public List<string> Check(OccupancyWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy window.</summary>
public sealed record OccupancyWindowValidator(List<IOccupancyWindowRule> Rules)
{
    public List<string> Validate(OccupancyWindow item)
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

    public void RequireValid(OccupancyWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyWindow> items)
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

/// <summary>Functions of occupancy window rules.</summary>
public static class OccupancyWindowRules
{
    public static OccupancyWindowValidator DefaultOccupancyWindowValidator()
    {
        return new OccupancyWindowValidator(
            new List<IOccupancyWindowRule>
            {
                new OccupancyWindowRequiredLabelRule("occupancy window needs this field"),
                new OccupancyWindowHoursRangeRule(1.4, 3450.7),
                new OccupancyWindowAllowedCodeRule(new List<string>()),
                new OccupancyWindowSkillLengthRule(80),
                new OccupancyWindowLabelLengthRule(255),
                new OccupancyWindowRequiredCodeRule("occupancy window needs this field"),
            });
    }
}
