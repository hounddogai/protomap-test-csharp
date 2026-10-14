namespace Acme.Workforce.Assets.Assignment.Snapshot;

/// <summary>A rule a default assignment snapshot must pass. Check returns its problems, or none.</summary>
public interface IDefaultAssignmentSnapshotRule
{
    List<string> Check(DefaultAssignmentSnapshot item);
}

public sealed record DefaultAssignmentSnapshotLabelLengthRule(int MaxLength) : IDefaultAssignmentSnapshotRule
{
    public List<string> Check(DefaultAssignmentSnapshot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAssignmentSnapshotRequiredCodeRule(string Message) : IDefaultAssignmentSnapshotRule
{
    public List<string> Check(DefaultAssignmentSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultAssignmentSnapshotRequiredDepartmentRule(string Message) : IDefaultAssignmentSnapshotRule
{
    public List<string> Check(DefaultAssignmentSnapshot item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultAssignmentSnapshotCodeLengthRule(int MaxLength) : IDefaultAssignmentSnapshotRule
{
    public List<string> Check(DefaultAssignmentSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAssignmentSnapshotContactEmailLengthRule(int MaxLength) : IDefaultAssignmentSnapshotRule
{
    public List<string> Check(DefaultAssignmentSnapshot item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default assignment snapshot.</summary>
public sealed record DefaultAssignmentSnapshotValidator(List<IDefaultAssignmentSnapshotRule> Rules)
{
    public List<string> Validate(DefaultAssignmentSnapshot item)
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

    public void RequireValid(DefaultAssignmentSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultAssignmentSnapshot> items)
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

/// <summary>Functions of default assignment snapshot rules.</summary>
public static class DefaultAssignmentSnapshotRules
{
    public static DefaultAssignmentSnapshotValidator DefaultDefaultAssignmentSnapshotValidator()
    {
        return new DefaultAssignmentSnapshotValidator(
            new List<IDefaultAssignmentSnapshotRule>
            {
                new DefaultAssignmentSnapshotLabelLengthRule(64),
                new DefaultAssignmentSnapshotRequiredCodeRule("default assignment snapshot needs a value"),
                new DefaultAssignmentSnapshotRequiredDepartmentRule("default assignment snapshot needs an entry"),
                new DefaultAssignmentSnapshotCodeLengthRule(80),
                new DefaultAssignmentSnapshotContactEmailLengthRule(255),
            });
    }
}
