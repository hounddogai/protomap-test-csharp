namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a attestation budget must pass. Check returns its problems, or none.</summary>
public interface IAttestationBudgetRule
{
    List<string> Check(AttestationBudget item);
}

public sealed record AttestationBudgetCodeLengthRule(int MaxLength) : IAttestationBudgetRule
{
    public List<string> Check(AttestationBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationBudgetRequiredLabelRule(string Message) : IAttestationBudgetRule
{
    public List<string> Check(AttestationBudget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationBudgetAllowedChannelRule(List<string> Allowed) : IAttestationBudgetRule
{
    public List<string> Check(AttestationBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record AttestationBudgetCodePrefixRule(string Prefix) : IAttestationBudgetRule
{
    public List<string> Check(AttestationBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationBudgetRequiredChannelRule(string Message) : IAttestationBudgetRule
{
    public List<string> Check(AttestationBudget item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationBudgetChannelLengthRule(int MaxLength) : IAttestationBudgetRule
{
    public List<string> Check(AttestationBudget item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation budget.</summary>
public sealed record AttestationBudgetValidator(List<IAttestationBudgetRule> Rules)
{
    public List<string> Validate(AttestationBudget item)
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

    public void RequireValid(AttestationBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationBudget> items)
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

/// <summary>Functions of attestation budget rules.</summary>
public static class AttestationBudgetRules
{
    public static AttestationBudgetValidator DefaultAttestationBudgetValidator()
    {
        return new AttestationBudgetValidator(
            new List<IAttestationBudgetRule>
            {
                new AttestationBudgetCodeLengthRule(255),
                new AttestationBudgetRequiredLabelRule("attestation budget needs an entry"),
                new AttestationBudgetAllowedChannelRule(new List<string>()),
                new AttestationBudgetCodePrefixRule("OPS"),
                new AttestationBudgetRequiredChannelRule("attestation budget needs a value"),
                new AttestationBudgetChannelLengthRule(1000),
            });
    }
}
