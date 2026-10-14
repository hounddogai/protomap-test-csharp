namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a attestation threshold must pass. Check returns its problems, or none.</summary>
public interface IAttestationThresholdRule
{
    List<string> Check(AttestationThreshold item);
}

public sealed record AttestationThresholdCodeLengthRule(int MaxLength) : IAttestationThresholdRule
{
    public List<string> Check(AttestationThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationThresholdNameLengthRule(int MaxLength) : IAttestationThresholdRule
{
    public List<string> Check(AttestationThreshold item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationThresholdRequiredChannelRule(string Message) : IAttestationThresholdRule
{
    public List<string> Check(AttestationThreshold item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationThresholdAllowedChannelRule(List<string> Allowed) : IAttestationThresholdRule
{
    public List<string> Check(AttestationThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record AttestationThresholdRequiredExternalRefRule(string Message) : IAttestationThresholdRule
{
    public List<string> Check(AttestationThreshold item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationThresholdRequiredCodeRule(string Message) : IAttestationThresholdRule
{
    public List<string> Check(AttestationThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation threshold.</summary>
public sealed record AttestationThresholdValidator(List<IAttestationThresholdRule> Rules)
{
    public List<string> Validate(AttestationThreshold item)
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

    public void RequireValid(AttestationThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationThreshold> items)
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

/// <summary>Functions of attestation threshold rules.</summary>
public static class AttestationThresholdRules
{
    public static AttestationThresholdValidator DefaultAttestationThresholdValidator()
    {
        return new AttestationThresholdValidator(
            new List<IAttestationThresholdRule>
            {
                new AttestationThresholdCodeLengthRule(120),
                new AttestationThresholdNameLengthRule(86400),
                new AttestationThresholdRequiredChannelRule("attestation threshold needs this field"),
                new AttestationThresholdAllowedChannelRule(new List<string> { "lenient" }),
                new AttestationThresholdRequiredExternalRefRule("attestation threshold needs an entry"),
                new AttestationThresholdRequiredCodeRule("attestation threshold needs a value"),
            });
    }
}
