namespace Acme.Workforce.Absence.Accrual.Assignment;

/// <summary>A rule a accrual assignment must pass. Check returns its problems, or none.</summary>
public interface IAccrualAssignmentRule
{
    List<string> Check(AccrualAssignment item);
}

public sealed record AccrualAssignmentRequiredChecksumRule(string Message) : IAccrualAssignmentRule
{
    public List<string> Check(AccrualAssignment item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualAssignmentTagsLimitRule(int MaxCount, List<string> Banned) : IAccrualAssignmentRule
{
    public List<string> Check(AccrualAssignment item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record AccrualAssignmentRequiredCodeRule(string Message) : IAccrualAssignmentRule
{
    public List<string> Check(AccrualAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualAssignmentRequiredTitleRule(string Message) : IAccrualAssignmentRule
{
    public List<string> Check(AccrualAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualAssignmentTitleLengthRule(int MaxLength) : IAccrualAssignmentRule
{
    public List<string> Check(AccrualAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual assignment.</summary>
public sealed record AccrualAssignmentValidator(List<IAccrualAssignmentRule> Rules)
{
    public List<string> Validate(AccrualAssignment item)
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

    public void RequireValid(AccrualAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualAssignment> items)
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

/// <summary>Functions of accrual assignment rules.</summary>
public static class AccrualAssignmentRules
{
    public static AccrualAssignmentValidator DefaultAccrualAssignmentValidator()
    {
        return new AccrualAssignmentValidator(
            new List<IAccrualAssignmentRule>
            {
                new AccrualAssignmentRequiredChecksumRule("accrual assignment needs this field"),
                new AccrualAssignmentTagsLimitRule(86400, new List<string> { "capped" }),
                new AccrualAssignmentRequiredCodeRule("accrual assignment needs this field"),
                new AccrualAssignmentRequiredTitleRule("accrual assignment needs this field"),
                new AccrualAssignmentTitleLengthRule(32),
            });
    }
}
