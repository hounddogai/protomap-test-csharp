namespace Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>A rule a assignment notice must pass. Check returns its problems, or none.</summary>
public interface IAssignmentNoticeRule
{
    List<string> Check(AssignmentNotice item);
}

public sealed record AssignmentNoticeRequiredChecksumRule(string Message) : IAssignmentNoticeRule
{
    public List<string> Check(AssignmentNotice item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentNoticeRequiredCodeRule(string Message) : IAssignmentNoticeRule
{
    public List<string> Check(AssignmentNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentNoticeDepartmentLengthRule(int MaxLength) : IAssignmentNoticeRule
{
    public List<string> Check(AssignmentNotice item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentNoticeTitleLengthRule(int MaxLength) : IAssignmentNoticeRule
{
    public List<string> Check(AssignmentNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment notice.</summary>
public sealed record AssignmentNoticeValidator(List<IAssignmentNoticeRule> Rules)
{
    public List<string> Validate(AssignmentNotice item)
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

    public void RequireValid(AssignmentNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentNotice> items)
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

/// <summary>Functions of assignment notice rules.</summary>
public static class AssignmentNoticeRules
{
    public static AssignmentNoticeValidator DefaultAssignmentNoticeValidator()
    {
        return new AssignmentNoticeValidator(
            new List<IAssignmentNoticeRule>
            {
                new AssignmentNoticeRequiredChecksumRule("assignment notice needs a value"),
                new AssignmentNoticeRequiredCodeRule("assignment notice needs this field"),
                new AssignmentNoticeDepartmentLengthRule(3600),
                new AssignmentNoticeTitleLengthRule(120),
            });
    }
}
