namespace Acme.Workforce.Assets.Assignment.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled assignment estimate must pass. Check returns its problems, or none.</summary>
public interface IScheduledAssignmentEstimateRule
{
    List<string> Check(ScheduledAssignmentEstimate item);
}

public sealed record ScheduledAssignmentEstimateRevisionRangeRule(int Min, int Max) : IScheduledAssignmentEstimateRule
{
    public List<string> Check(ScheduledAssignmentEstimate item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledAssignmentEstimateAllowedSkillRule(List<string> Allowed) : IScheduledAssignmentEstimateRule
{
    public List<string> Check(ScheduledAssignmentEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssignmentEstimateSkillLengthRule(int MaxLength) : IScheduledAssignmentEstimateRule
{
    public List<string> Check(ScheduledAssignmentEstimate item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssignmentEstimateRequiredLabelRule(string Message) : IScheduledAssignmentEstimateRule
{
    public List<string> Check(ScheduledAssignmentEstimate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssignmentEstimateRetriesRangeRule(int Min, int Max) : IScheduledAssignmentEstimateRule
{
    public List<string> Check(ScheduledAssignmentEstimate item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssignmentEstimateCodeLengthRule(int MaxLength) : IScheduledAssignmentEstimateRule
{
    public List<string> Check(ScheduledAssignmentEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled assignment estimate.</summary>
public sealed record ScheduledAssignmentEstimateValidator(List<IScheduledAssignmentEstimateRule> Rules)
{
    public List<string> Validate(ScheduledAssignmentEstimate item)
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

    public void RequireValid(ScheduledAssignmentEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAssignmentEstimate> items)
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

/// <summary>Functions of scheduled assignment estimate rules.</summary>
public static class ScheduledAssignmentEstimateRules
{
    public static ScheduledAssignmentEstimateValidator DefaultScheduledAssignmentEstimateValidator()
    {
        return new ScheduledAssignmentEstimateValidator(
            new List<IScheduledAssignmentEstimateRule>
            {
                new ScheduledAssignmentEstimateRevisionRangeRule(16, 64),
                new ScheduledAssignmentEstimateAllowedSkillRule(new List<string>()),
                new ScheduledAssignmentEstimateSkillLengthRule(64),
                new ScheduledAssignmentEstimateRequiredLabelRule("scheduled assignment estimate needs this field"),
                new ScheduledAssignmentEstimateRetriesRangeRule(16, 86400),
                new ScheduledAssignmentEstimateCodeLengthRule(64),
            });
    }
}
