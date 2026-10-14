namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

/// <summary>A rule a assignment item must pass. Check returns its problems, or none.</summary>
public interface IAssignmentItemRule
{
    List<string> Check(AssignmentItem item);
}

public sealed record AssignmentItemRequiredTitleRule(string Message) : IAssignmentItemRule
{
    public List<string> Check(AssignmentItem item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentItemHoursRangeRule(double Min, double Max) : IAssignmentItemRule
{
    public List<string> Check(AssignmentItem item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentItemChecksumLengthRule(int MaxLength) : IAssignmentItemRule
{
    public List<string> Check(AssignmentItem item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentItemCategoryLengthRule(int MaxLength) : IAssignmentItemRule
{
    public List<string> Check(AssignmentItem item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentItemCodeLengthRule(int MaxLength) : IAssignmentItemRule
{
    public List<string> Check(AssignmentItem item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment item.</summary>
public sealed record AssignmentItemValidator(List<IAssignmentItemRule> Rules)
{
    public List<string> Validate(AssignmentItem item)
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

    public void RequireValid(AssignmentItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentItem> items)
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

/// <summary>Functions of assignment item rules.</summary>
public static class AssignmentItemRules
{
    public static AssignmentItemValidator DefaultAssignmentItemValidator()
    {
        return new AssignmentItemValidator(
            new List<IAssignmentItemRule>
            {
                new AssignmentItemRequiredTitleRule("assignment item needs a value"),
                new AssignmentItemHoursRangeRule(2.0, 766.0),
                new AssignmentItemChecksumLengthRule(64),
                new AssignmentItemCategoryLengthRule(3600),
                new AssignmentItemCodeLengthRule(86400),
            });
    }
}
