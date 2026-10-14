namespace Acme.Workforce.Compensation.EquityGrant.Digest;

/// <summary>A rule a equity grant digest must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantDigestRule
{
    List<string> Check(EquityGrantDigest item);
}

public sealed record EquityGrantDigestRequiredLabelRule(string Message) : IEquityGrantDigestRule
{
    public List<string> Check(EquityGrantDigest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantDigestRequiredDepartmentRule(string Message) : IEquityGrantDigestRule
{
    public List<string> Check(EquityGrantDigest item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantDigestRequiredCodeRule(string Message) : IEquityGrantDigestRule
{
    public List<string> Check(EquityGrantDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantDigestLabelLengthRule(int MaxLength) : IEquityGrantDigestRule
{
    public List<string> Check(EquityGrantDigest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant digest.</summary>
public sealed record EquityGrantDigestValidator(List<IEquityGrantDigestRule> Rules)
{
    public List<string> Validate(EquityGrantDigest item)
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

    public void RequireValid(EquityGrantDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantDigest> items)
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

/// <summary>Functions of equity grant digest rules.</summary>
public static class EquityGrantDigestRules
{
    public static EquityGrantDigestValidator DefaultEquityGrantDigestValidator()
    {
        return new EquityGrantDigestValidator(
            new List<IEquityGrantDigestRule>
            {
                new EquityGrantDigestRequiredLabelRule("equity grant digest needs an entry"),
                new EquityGrantDigestRequiredDepartmentRule("equity grant digest needs this field"),
                new EquityGrantDigestRequiredCodeRule("equity grant digest needs this field"),
                new EquityGrantDigestLabelLengthRule(64),
            });
    }
}
