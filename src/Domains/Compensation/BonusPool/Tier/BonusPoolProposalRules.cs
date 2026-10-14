namespace Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>A rule a bonus pool proposal must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolProposalRule
{
    List<string> Check(BonusPoolProposal item);
}

public sealed record BonusPoolProposalRateRangeRule(double Min, double Max) : IBonusPoolProposalRule
{
    public List<string> Check(BonusPoolProposal item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BonusPoolProposalBadgeLengthRule(int MaxLength) : IBonusPoolProposalRule
{
    public List<string> Check(BonusPoolProposal item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolProposalApprovedGuardRule(int Limit) : IBonusPoolProposalRule
{
    public List<string> Check(BonusPoolProposal item)
    {
        if (item.Approved && item.Retries > Limit)
        {
            return new List<string> { $"retries {item.Retries} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolProposalCodePrefixRule(string Prefix) : IBonusPoolProposalRule
{
    public List<string> Check(BonusPoolProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool proposal.</summary>
public sealed record BonusPoolProposalValidator(List<IBonusPoolProposalRule> Rules)
{
    public List<string> Validate(BonusPoolProposal item)
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

    public void RequireValid(BonusPoolProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolProposal> items)
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

/// <summary>Functions of bonus pool proposal rules.</summary>
public static class BonusPoolProposalRules
{
    public static BonusPoolProposalValidator DefaultBonusPoolProposalValidator()
    {
        return new BonusPoolProposalValidator(
            new List<IBonusPoolProposalRule>
            {
                new BonusPoolProposalRateRangeRule(4.5, 4111.0),
                new BonusPoolProposalBadgeLengthRule(3600),
                new BonusPoolProposalApprovedGuardRule(80),
                new BonusPoolProposalCodePrefixRule("OPS"),
            });
    }
}
