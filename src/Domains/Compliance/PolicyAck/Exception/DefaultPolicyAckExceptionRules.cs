namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default policy ack exception must pass. Check returns its problems, or none.</summary>
public interface IDefaultPolicyAckExceptionRule
{
    List<string> Check(DefaultPolicyAckException item);
}

public sealed record DefaultPolicyAckExceptionRequiredNameRule(string Message) : IDefaultPolicyAckExceptionRule
{
    public List<string> Check(DefaultPolicyAckException item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyAckExceptionAllowedCodeRule(List<string> Allowed) : IDefaultPolicyAckExceptionRule
{
    public List<string> Check(DefaultPolicyAckException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyAckExceptionCreatedAtRangeRule(int Min, int Max) : IDefaultPolicyAckExceptionRule
{
    public List<string> Check(DefaultPolicyAckException item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyAckExceptionCodeLengthRule(int MaxLength) : IDefaultPolicyAckExceptionRule
{
    public List<string> Check(DefaultPolicyAckException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyAckExceptionCodePrefixRule(string Prefix) : IDefaultPolicyAckExceptionRule
{
    public List<string> Check(DefaultPolicyAckException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyAckExceptionNameLengthRule(int MaxLength) : IDefaultPolicyAckExceptionRule
{
    public List<string> Check(DefaultPolicyAckException item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default policy ack exception.</summary>
public sealed record DefaultPolicyAckExceptionValidator(List<IDefaultPolicyAckExceptionRule> Rules)
{
    public List<string> Validate(DefaultPolicyAckException item)
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

    public void RequireValid(DefaultPolicyAckException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPolicyAckException> items)
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

/// <summary>Functions of default policy ack exception rules.</summary>
public static class DefaultPolicyAckExceptionRules
{
    public static DefaultPolicyAckExceptionValidator DefaultDefaultPolicyAckExceptionValidator()
    {
        return new DefaultPolicyAckExceptionValidator(
            new List<IDefaultPolicyAckExceptionRule>
            {
                new DefaultPolicyAckExceptionRequiredNameRule("default policy ack exception needs this field"),
                new DefaultPolicyAckExceptionAllowedCodeRule(new List<string> { "batched" }),
                new DefaultPolicyAckExceptionCreatedAtRangeRule(120, 86400),
                new DefaultPolicyAckExceptionCodeLengthRule(16),
                new DefaultPolicyAckExceptionCodePrefixRule("ACM"),
                new DefaultPolicyAckExceptionNameLengthRule(3600),
            });
    }
}
