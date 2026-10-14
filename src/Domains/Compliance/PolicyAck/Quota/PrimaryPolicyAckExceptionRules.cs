namespace Acme.Workforce.Compliance.PolicyAck.Quota;

/// <summary>A rule a primary policy ack exception must pass. Check returns its problems, or none.</summary>
public interface IPrimaryPolicyAckExceptionRule
{
    List<string> Check(PrimaryPolicyAckException item);
}

public sealed record PrimaryPolicyAckExceptionCodeLengthRule(int MaxLength) : IPrimaryPolicyAckExceptionRule
{
    public List<string> Check(PrimaryPolicyAckException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckExceptionWindowDaysRangeRule(int Min, int Max) : IPrimaryPolicyAckExceptionRule
{
    public List<string> Check(PrimaryPolicyAckException item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckExceptionJobCodeLengthRule(int MaxLength) : IPrimaryPolicyAckExceptionRule
{
    public List<string> Check(PrimaryPolicyAckException item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyAckExceptionTagsLimitRule(int MaxCount, List<string> Banned) : IPrimaryPolicyAckExceptionRule
{
    public List<string> Check(PrimaryPolicyAckException item)
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

/// <summary>Runs every rule of a primary policy ack exception.</summary>
public sealed record PrimaryPolicyAckExceptionValidator(List<IPrimaryPolicyAckExceptionRule> Rules)
{
    public List<string> Validate(PrimaryPolicyAckException item)
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

    public void RequireValid(PrimaryPolicyAckException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryPolicyAckException> items)
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

/// <summary>Functions of primary policy ack exception rules.</summary>
public static class PrimaryPolicyAckExceptionRules
{
    public static PrimaryPolicyAckExceptionValidator DefaultPrimaryPolicyAckExceptionValidator()
    {
        return new PrimaryPolicyAckExceptionValidator(
            new List<IPrimaryPolicyAckExceptionRule>
            {
                new PrimaryPolicyAckExceptionCodeLengthRule(80),
                new PrimaryPolicyAckExceptionWindowDaysRangeRule(64, 255),
                new PrimaryPolicyAckExceptionJobCodeLengthRule(86400),
                new PrimaryPolicyAckExceptionTagsLimitRule(1000, new List<string> { "regional", "rolling" }),
            });
    }
}
