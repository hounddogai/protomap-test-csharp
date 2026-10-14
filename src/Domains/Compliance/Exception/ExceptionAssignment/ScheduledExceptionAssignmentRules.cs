namespace Acme.Workforce.Compliance.Exception.ExceptionAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled exception assignment must pass. Check returns its problems, or none.</summary>
public interface IScheduledExceptionAssignmentRule
{
    List<string> Check(ScheduledExceptionAssignment item);
}

public sealed record ScheduledExceptionAssignmentAllowedSourceRule(List<string> Allowed) : IScheduledExceptionAssignmentRule
{
    public List<string> Check(ScheduledExceptionAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledExceptionAssignmentTitleLengthRule(int MaxLength) : IScheduledExceptionAssignmentRule
{
    public List<string> Check(ScheduledExceptionAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledExceptionAssignmentRequiredCodeRule(string Message) : IScheduledExceptionAssignmentRule
{
    public List<string> Check(ScheduledExceptionAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledExceptionAssignmentRequiredTitleRule(string Message) : IScheduledExceptionAssignmentRule
{
    public List<string> Check(ScheduledExceptionAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledExceptionAssignmentCodeLengthRule(int MaxLength) : IScheduledExceptionAssignmentRule
{
    public List<string> Check(ScheduledExceptionAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled exception assignment.</summary>
public sealed record ScheduledExceptionAssignmentValidator(List<IScheduledExceptionAssignmentRule> Rules)
{
    public List<string> Validate(ScheduledExceptionAssignment item)
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

    public void RequireValid(ScheduledExceptionAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledExceptionAssignment> items)
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

/// <summary>Functions of scheduled exception assignment rules.</summary>
public static class ScheduledExceptionAssignmentRules
{
    public static ScheduledExceptionAssignmentValidator DefaultScheduledExceptionAssignmentValidator()
    {
        return new ScheduledExceptionAssignmentValidator(
            new List<IScheduledExceptionAssignmentRule>
            {
                new ScheduledExceptionAssignmentAllowedSourceRule(new List<string> { "strict", "tiered" }),
                new ScheduledExceptionAssignmentTitleLengthRule(16),
                new ScheduledExceptionAssignmentRequiredCodeRule("scheduled exception assignment needs an entry"),
                new ScheduledExceptionAssignmentRequiredTitleRule("scheduled exception assignment needs this field"),
                new ScheduledExceptionAssignmentCodeLengthRule(32),
            });
    }
}
