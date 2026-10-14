namespace Acme.Workforce.Orgdesign.Headcount.Agreement;

/// <summary>A rule a headcount checkpoint must pass. Check returns its problems, or none.</summary>
public interface IHeadcountCheckpointRule
{
    List<string> Check(HeadcountCheckpoint item);
}

public sealed record HeadcountCheckpointRequiredCodeRule(string Message) : IHeadcountCheckpointRule
{
    public List<string> Check(HeadcountCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountCheckpointRequiredNameRule(string Message) : IHeadcountCheckpointRule
{
    public List<string> Check(HeadcountCheckpoint item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountCheckpointContactEmailLengthRule(int MaxLength) : IHeadcountCheckpointRule
{
    public List<string> Check(HeadcountCheckpoint item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountCheckpointRequiredCategoryRule(string Message) : IHeadcountCheckpointRule
{
    public List<string> Check(HeadcountCheckpoint item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountCheckpointCategoryLengthRule(int MaxLength) : IHeadcountCheckpointRule
{
    public List<string> Check(HeadcountCheckpoint item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount checkpoint.</summary>
public sealed record HeadcountCheckpointValidator(List<IHeadcountCheckpointRule> Rules)
{
    public List<string> Validate(HeadcountCheckpoint item)
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

    public void RequireValid(HeadcountCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountCheckpoint> items)
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

/// <summary>Functions of headcount checkpoint rules.</summary>
public static class HeadcountCheckpointRules
{
    public static HeadcountCheckpointValidator DefaultHeadcountCheckpointValidator()
    {
        return new HeadcountCheckpointValidator(
            new List<IHeadcountCheckpointRule>
            {
                new HeadcountCheckpointRequiredCodeRule("headcount checkpoint needs an entry"),
                new HeadcountCheckpointRequiredNameRule("headcount checkpoint needs this field"),
                new HeadcountCheckpointContactEmailLengthRule(16),
                new HeadcountCheckpointRequiredCategoryRule("headcount checkpoint needs this field"),
                new HeadcountCheckpointCategoryLengthRule(80),
            });
    }
}
