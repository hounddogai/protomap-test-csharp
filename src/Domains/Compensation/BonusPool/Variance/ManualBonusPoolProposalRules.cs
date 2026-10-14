namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual bonus pool proposal must pass. Check returns its problems, or none.</summary>
public interface IManualBonusPoolProposalRule
{
    List<string> Check(ManualBonusPoolProposal item);
}

public sealed record ManualBonusPoolProposalNameLengthRule(int MaxLength) : IManualBonusPoolProposalRule
{
    public List<string> Check(ManualBonusPoolProposal item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBonusPoolProposalCodePrefixRule(string Prefix) : IManualBonusPoolProposalRule
{
    public List<string> Check(ManualBonusPoolProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBonusPoolProposalBadgeLengthRule(int MaxLength) : IManualBonusPoolProposalRule
{
    public List<string> Check(ManualBonusPoolProposal item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBonusPoolProposalAllowedNameRule(List<string> Allowed) : IManualBonusPoolProposalRule
{
    public List<string> Check(ManualBonusPoolProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record ManualBonusPoolProposalRequiredNameRule(string Message) : IManualBonusPoolProposalRule
{
    public List<string> Check(ManualBonusPoolProposal item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual bonus pool proposal.</summary>
public sealed record ManualBonusPoolProposalValidator(List<IManualBonusPoolProposalRule> Rules)
{
    public List<string> Validate(ManualBonusPoolProposal item)
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

    public void RequireValid(ManualBonusPoolProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualBonusPoolProposal> items)
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

/// <summary>Functions of manual bonus pool proposal rules.</summary>
public static class ManualBonusPoolProposalRules
{
    public static ManualBonusPoolProposalValidator DefaultManualBonusPoolProposalValidator()
    {
        return new ManualBonusPoolProposalValidator(
            new List<IManualBonusPoolProposalRule>
            {
                new ManualBonusPoolProposalNameLengthRule(1000),
                new ManualBonusPoolProposalCodePrefixRule("ACM"),
                new ManualBonusPoolProposalBadgeLengthRule(16),
                new ManualBonusPoolProposalAllowedNameRule(new List<string> { "seasonal" }),
                new ManualBonusPoolProposalRequiredNameRule("manual bonus pool proposal needs a value"),
            });
    }
}
