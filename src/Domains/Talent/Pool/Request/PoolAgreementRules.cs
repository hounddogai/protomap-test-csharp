namespace Acme.Workforce.Talent.Pool.Request;

/// <summary>A rule a pool agreement must pass. Check returns its problems, or none.</summary>
public interface IPoolAgreementRule
{
    List<string> Check(PoolAgreement item);
}

public sealed record PoolAgreementRequiredSegmentRule(string Message) : IPoolAgreementRule
{
    public List<string> Check(PoolAgreement item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolAgreementLabelLengthRule(int MaxLength) : IPoolAgreementRule
{
    public List<string> Check(PoolAgreement item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolAgreementRequiredTitleRule(string Message) : IPoolAgreementRule
{
    public List<string> Check(PoolAgreement item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolAgreementRequiredCodeRule(string Message) : IPoolAgreementRule
{
    public List<string> Check(PoolAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolAgreementCodePrefixRule(string Prefix) : IPoolAgreementRule
{
    public List<string> Check(PoolAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PoolAgreementCodeLengthRule(int MaxLength) : IPoolAgreementRule
{
    public List<string> Check(PoolAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool agreement.</summary>
public sealed record PoolAgreementValidator(List<IPoolAgreementRule> Rules)
{
    public List<string> Validate(PoolAgreement item)
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

    public void RequireValid(PoolAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolAgreement> items)
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

/// <summary>Functions of pool agreement rules.</summary>
public static class PoolAgreementRules
{
    public static PoolAgreementValidator DefaultPoolAgreementValidator()
    {
        return new PoolAgreementValidator(
            new List<IPoolAgreementRule>
            {
                new PoolAgreementRequiredSegmentRule("pool agreement needs a value"),
                new PoolAgreementLabelLengthRule(86400),
                new PoolAgreementRequiredTitleRule("pool agreement needs this field"),
                new PoolAgreementRequiredCodeRule("pool agreement needs a value"),
                new PoolAgreementCodePrefixRule("ACM"),
                new PoolAgreementCodeLengthRule(255),
            });
    }
}
