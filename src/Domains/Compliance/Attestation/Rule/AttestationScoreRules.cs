namespace Acme.Workforce.Compliance.Attestation.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a attestation score must pass. Check returns its problems, or none.</summary>
public interface IAttestationScoreRule
{
    List<string> Check(AttestationScore item);
}

public sealed record AttestationScoreRequiredTitleRule(string Message) : IAttestationScoreRule
{
    public List<string> Check(AttestationScore item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationScoreAllowedRegionRule(List<string> Allowed) : IAttestationScoreRule
{
    public List<string> Check(AttestationScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record AttestationScoreCodePrefixRule(string Prefix) : IAttestationScoreRule
{
    public List<string> Check(AttestationScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation score.</summary>
public sealed record AttestationScoreValidator(List<IAttestationScoreRule> Rules)
{
    public List<string> Validate(AttestationScore item)
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

    public void RequireValid(AttestationScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationScore> items)
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

/// <summary>Functions of attestation score rules.</summary>
public static class AttestationScoreRules
{
    public static AttestationScoreValidator DefaultAttestationScoreValidator()
    {
        return new AttestationScoreValidator(
            new List<IAttestationScoreRule>
            {
                new AttestationScoreRequiredTitleRule("attestation score needs this field"),
                new AttestationScoreAllowedRegionRule(new List<string> { "pending" }),
                new AttestationScoreCodePrefixRule("ACM"),
            });
    }
}
