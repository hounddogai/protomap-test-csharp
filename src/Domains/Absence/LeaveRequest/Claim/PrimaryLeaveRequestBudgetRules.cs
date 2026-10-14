namespace Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>A rule a primary leave request budget must pass. Check returns its problems, or none.</summary>
public interface IPrimaryLeaveRequestBudgetRule
{
    List<string> Check(PrimaryLeaveRequestBudget item);
}

public sealed record PrimaryLeaveRequestBudgetBatchSizeRangeRule(int Min, int Max) : IPrimaryLeaveRequestBudgetRule
{
    public List<string> Check(PrimaryLeaveRequestBudget item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLeaveRequestBudgetExternalRefLengthRule(int MaxLength) : IPrimaryLeaveRequestBudgetRule
{
    public List<string> Check(PrimaryLeaveRequestBudget item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLeaveRequestBudgetNameLengthRule(int MaxLength) : IPrimaryLeaveRequestBudgetRule
{
    public List<string> Check(PrimaryLeaveRequestBudget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLeaveRequestBudgetTagsLimitRule(int MaxCount, List<string> Banned) : IPrimaryLeaveRequestBudgetRule
{
    public List<string> Check(PrimaryLeaveRequestBudget item)
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

public sealed record PrimaryLeaveRequestBudgetCreatedAtRangeRule(int Min, int Max) : IPrimaryLeaveRequestBudgetRule
{
    public List<string> Check(PrimaryLeaveRequestBudget item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLeaveRequestBudgetRequiredNameRule(string Message) : IPrimaryLeaveRequestBudgetRule
{
    public List<string> Check(PrimaryLeaveRequestBudget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary leave request budget.</summary>
public sealed record PrimaryLeaveRequestBudgetValidator(List<IPrimaryLeaveRequestBudgetRule> Rules)
{
    public List<string> Validate(PrimaryLeaveRequestBudget item)
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

    public void RequireValid(PrimaryLeaveRequestBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryLeaveRequestBudget> items)
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

/// <summary>Functions of primary leave request budget rules.</summary>
public static class PrimaryLeaveRequestBudgetRules
{
    public static PrimaryLeaveRequestBudgetValidator DefaultPrimaryLeaveRequestBudgetValidator()
    {
        return new PrimaryLeaveRequestBudgetValidator(
            new List<IPrimaryLeaveRequestBudgetRule>
            {
                new PrimaryLeaveRequestBudgetBatchSizeRangeRule(32, 3600),
                new PrimaryLeaveRequestBudgetExternalRefLengthRule(32),
                new PrimaryLeaveRequestBudgetNameLengthRule(32),
                new PrimaryLeaveRequestBudgetTagsLimitRule(32, new List<string>()),
                new PrimaryLeaveRequestBudgetCreatedAtRangeRule(120, 86400),
                new PrimaryLeaveRequestBudgetRequiredNameRule("primary leave request budget needs this field"),
            });
    }
}
