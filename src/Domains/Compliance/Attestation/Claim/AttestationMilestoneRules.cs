namespace Acme.Workforce.Compliance.Attestation.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a attestation milestone must pass. Check returns its problems, or none.</summary>
public interface IAttestationMilestoneRule
{
    List<string> Check(AttestationMilestone item);
}

public sealed record AttestationMilestoneAllowedCodeRule(List<string> Allowed) : IAttestationMilestoneRule
{
    public List<string> Check(AttestationMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record AttestationMilestoneLabelLengthRule(int MaxLength) : IAttestationMilestoneRule
{
    public List<string> Check(AttestationMilestone item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationMilestoneCodePrefixRule(string Prefix) : IAttestationMilestoneRule
{
    public List<string> Check(AttestationMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationMilestoneNameLengthRule(int MaxLength) : IAttestationMilestoneRule
{
    public List<string> Check(AttestationMilestone item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationMilestoneRatioRangeRule(double Min, double Max) : IAttestationMilestoneRule
{
    public List<string> Check(AttestationMilestone item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation milestone.</summary>
public sealed record AttestationMilestoneValidator(List<IAttestationMilestoneRule> Rules)
{
    public List<string> Validate(AttestationMilestone item)
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

    public void RequireValid(AttestationMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationMilestone> items)
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

/// <summary>Functions of attestation milestone rules.</summary>
public static class AttestationMilestoneRules
{
    public static AttestationMilestoneValidator DefaultAttestationMilestoneValidator()
    {
        return new AttestationMilestoneValidator(
            new List<IAttestationMilestoneRule>
            {
                new AttestationMilestoneAllowedCodeRule(new List<string>()),
                new AttestationMilestoneLabelLengthRule(80),
                new AttestationMilestoneCodePrefixRule("ACM"),
                new AttestationMilestoneNameLengthRule(16),
                new AttestationMilestoneRatioRangeRule(6.6, 3001.9),
            });
    }
}
