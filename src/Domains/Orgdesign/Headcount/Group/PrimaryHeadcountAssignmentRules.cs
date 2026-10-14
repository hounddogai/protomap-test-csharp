namespace Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>A rule a primary headcount assignment must pass. Check returns its problems, or none.</summary>
public interface IPrimaryHeadcountAssignmentRule
{
    List<string> Check(PrimaryHeadcountAssignment item);
}

public sealed record PrimaryHeadcountAssignmentRequiredOwnerIdRule(string Message) : IPrimaryHeadcountAssignmentRule
{
    public List<string> Check(PrimaryHeadcountAssignment item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryHeadcountAssignmentChannelLengthRule(int MaxLength) : IPrimaryHeadcountAssignmentRule
{
    public List<string> Check(PrimaryHeadcountAssignment item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryHeadcountAssignmentOwnerIdLengthRule(int MaxLength) : IPrimaryHeadcountAssignmentRule
{
    public List<string> Check(PrimaryHeadcountAssignment item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary headcount assignment.</summary>
public sealed record PrimaryHeadcountAssignmentValidator(List<IPrimaryHeadcountAssignmentRule> Rules)
{
    public List<string> Validate(PrimaryHeadcountAssignment item)
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

    public void RequireValid(PrimaryHeadcountAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryHeadcountAssignment> items)
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

/// <summary>Functions of primary headcount assignment rules.</summary>
public static class PrimaryHeadcountAssignmentRules
{
    public static PrimaryHeadcountAssignmentValidator DefaultPrimaryHeadcountAssignmentValidator()
    {
        return new PrimaryHeadcountAssignmentValidator(
            new List<IPrimaryHeadcountAssignmentRule>
            {
                new PrimaryHeadcountAssignmentRequiredOwnerIdRule("primary headcount assignment needs an entry"),
                new PrimaryHeadcountAssignmentChannelLengthRule(120),
                new PrimaryHeadcountAssignmentOwnerIdLengthRule(64),
            });
    }
}
