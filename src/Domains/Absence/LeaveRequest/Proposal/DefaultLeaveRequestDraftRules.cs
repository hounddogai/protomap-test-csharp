namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default leave request draft must pass. Check returns its problems, or none.</summary>
public interface IDefaultLeaveRequestDraftRule
{
    List<string> Check(DefaultLeaveRequestDraft item);
}

public sealed record DefaultLeaveRequestDraftDepartmentLengthRule(int MaxLength) : IDefaultLeaveRequestDraftRule
{
    public List<string> Check(DefaultLeaveRequestDraft item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultLeaveRequestDraftSegmentLengthRule(int MaxLength) : IDefaultLeaveRequestDraftRule
{
    public List<string> Check(DefaultLeaveRequestDraft item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultLeaveRequestDraftRequiredLabelRule(string Message) : IDefaultLeaveRequestDraftRule
{
    public List<string> Check(DefaultLeaveRequestDraft item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultLeaveRequestDraftAllowedLabelRule(List<string> Allowed) : IDefaultLeaveRequestDraftRule
{
    public List<string> Check(DefaultLeaveRequestDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record DefaultLeaveRequestDraftRequiredCodeRule(string Message) : IDefaultLeaveRequestDraftRule
{
    public List<string> Check(DefaultLeaveRequestDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default leave request draft.</summary>
public sealed record DefaultLeaveRequestDraftValidator(List<IDefaultLeaveRequestDraftRule> Rules)
{
    public List<string> Validate(DefaultLeaveRequestDraft item)
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

    public void RequireValid(DefaultLeaveRequestDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultLeaveRequestDraft> items)
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

/// <summary>Functions of default leave request draft rules.</summary>
public static class DefaultLeaveRequestDraftRules
{
    public static DefaultLeaveRequestDraftValidator DefaultDefaultLeaveRequestDraftValidator()
    {
        return new DefaultLeaveRequestDraftValidator(
            new List<IDefaultLeaveRequestDraftRule>
            {
                new DefaultLeaveRequestDraftDepartmentLengthRule(32),
                new DefaultLeaveRequestDraftSegmentLengthRule(32),
                new DefaultLeaveRequestDraftRequiredLabelRule("default leave request draft needs a value"),
                new DefaultLeaveRequestDraftAllowedLabelRule(new List<string> { "primary", "legacy", "tiered" }),
                new DefaultLeaveRequestDraftRequiredCodeRule("default leave request draft needs a value"),
            });
    }
}
