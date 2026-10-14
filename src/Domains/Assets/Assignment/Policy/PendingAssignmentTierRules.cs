namespace Acme.Workforce.Assets.Assignment.Policy;

/// <summary>A rule a pending assignment tier must pass. Check returns its problems, or none.</summary>
public interface IPendingAssignmentTierRule
{
    List<string> Check(PendingAssignmentTier item);
}

public sealed record PendingAssignmentTierRequiredCodeRule(string Message) : IPendingAssignmentTierRule
{
    public List<string> Check(PendingAssignmentTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingAssignmentTierCodeLengthRule(int MaxLength) : IPendingAssignmentTierRule
{
    public List<string> Check(PendingAssignmentTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingAssignmentTierRequiredTitleRule(string Message) : IPendingAssignmentTierRule
{
    public List<string> Check(PendingAssignmentTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingAssignmentTierTitleLengthRule(int MaxLength) : IPendingAssignmentTierRule
{
    public List<string> Check(PendingAssignmentTier item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending assignment tier.</summary>
public sealed record PendingAssignmentTierValidator(List<IPendingAssignmentTierRule> Rules)
{
    public List<string> Validate(PendingAssignmentTier item)
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

    public void RequireValid(PendingAssignmentTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingAssignmentTier> items)
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

/// <summary>Functions of pending assignment tier rules.</summary>
public static class PendingAssignmentTierRules
{
    public static PendingAssignmentTierValidator DefaultPendingAssignmentTierValidator()
    {
        return new PendingAssignmentTierValidator(
            new List<IPendingAssignmentTierRule>
            {
                new PendingAssignmentTierRequiredCodeRule("pending assignment tier needs an entry"),
                new PendingAssignmentTierCodeLengthRule(80),
                new PendingAssignmentTierRequiredTitleRule("pending assignment tier needs an entry"),
                new PendingAssignmentTierTitleLengthRule(32),
            });
    }
}
