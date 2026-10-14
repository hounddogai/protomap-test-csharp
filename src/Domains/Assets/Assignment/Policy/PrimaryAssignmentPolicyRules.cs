namespace Acme.Workforce.Assets.Assignment.Policy;

/// <summary>A rule a primary assignment policy must pass. Check returns its problems, or none.</summary>
public interface IPrimaryAssignmentPolicyRule
{
    List<string> Check(PrimaryAssignmentPolicy item);
}

public sealed record PrimaryAssignmentPolicyPhoneLengthRule(int MaxLength) : IPrimaryAssignmentPolicyRule
{
    public List<string> Check(PrimaryAssignmentPolicy item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAssignmentPolicyCodeLengthRule(int MaxLength) : IPrimaryAssignmentPolicyRule
{
    public List<string> Check(PrimaryAssignmentPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAssignmentPolicyCodePrefixRule(string Prefix) : IPrimaryAssignmentPolicyRule
{
    public List<string> Check(PrimaryAssignmentPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary assignment policy.</summary>
public sealed record PrimaryAssignmentPolicyValidator(List<IPrimaryAssignmentPolicyRule> Rules)
{
    public List<string> Validate(PrimaryAssignmentPolicy item)
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

    public void RequireValid(PrimaryAssignmentPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryAssignmentPolicy> items)
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

/// <summary>Functions of primary assignment policy rules.</summary>
public static class PrimaryAssignmentPolicyRules
{
    public static PrimaryAssignmentPolicyValidator DefaultPrimaryAssignmentPolicyValidator()
    {
        return new PrimaryAssignmentPolicyValidator(
            new List<IPrimaryAssignmentPolicyRule>
            {
                new PrimaryAssignmentPolicyPhoneLengthRule(80),
                new PrimaryAssignmentPolicyCodeLengthRule(86400),
                new PrimaryAssignmentPolicyCodePrefixRule("X"),
            });
    }
}
