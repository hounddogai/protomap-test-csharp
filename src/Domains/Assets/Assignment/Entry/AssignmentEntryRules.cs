namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a assignment entry must pass. Check returns its problems, or none.</summary>
public interface IAssignmentEntryRule
{
    List<string> Check(AssignmentEntry item);
}

public sealed record AssignmentEntryAllowedCodeRule(List<string> Allowed) : IAssignmentEntryRule
{
    public List<string> Check(AssignmentEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentEntryRequiredNameRule(string Message) : IAssignmentEntryRule
{
    public List<string> Check(AssignmentEntry item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentEntryNameLengthRule(int MaxLength) : IAssignmentEntryRule
{
    public List<string> Check(AssignmentEntry item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentEntryRequiredNotesRule(string Message) : IAssignmentEntryRule
{
    public List<string> Check(AssignmentEntry item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment entry.</summary>
public sealed record AssignmentEntryValidator(List<IAssignmentEntryRule> Rules)
{
    public List<string> Validate(AssignmentEntry item)
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

    public void RequireValid(AssignmentEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentEntry> items)
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

/// <summary>Functions of assignment entry rules.</summary>
public static class AssignmentEntryRules
{
    public static AssignmentEntryValidator DefaultAssignmentEntryValidator()
    {
        return new AssignmentEntryValidator(
            new List<IAssignmentEntryRule>
            {
                new AssignmentEntryAllowedCodeRule(new List<string>()),
                new AssignmentEntryRequiredNameRule("assignment entry needs an entry"),
                new AssignmentEntryNameLengthRule(64),
                new AssignmentEntryRequiredNotesRule("assignment entry needs this field"),
            });
    }
}
