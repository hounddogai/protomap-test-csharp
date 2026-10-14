namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy bonus pool proposal must pass. Check returns its problems, or none.</summary>
public interface ILegacyBonusPoolProposalRule
{
    List<string> Check(LegacyBonusPoolProposal item);
}

public sealed record LegacyBonusPoolProposalRequiredCodeRule(string Message) : ILegacyBonusPoolProposalRule
{
    public List<string> Check(LegacyBonusPoolProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyBonusPoolProposalBadgeLengthRule(int MaxLength) : ILegacyBonusPoolProposalRule
{
    public List<string> Check(LegacyBonusPoolProposal item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyBonusPoolProposalLimitRangeRule(int Min, int Max) : ILegacyBonusPoolProposalRule
{
    public List<string> Check(LegacyBonusPoolProposal item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyBonusPoolProposalRetriesRangeRule(int Min, int Max) : ILegacyBonusPoolProposalRule
{
    public List<string> Check(LegacyBonusPoolProposal item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyBonusPoolProposalAllowedLabelRule(List<string> Allowed) : ILegacyBonusPoolProposalRule
{
    public List<string> Check(LegacyBonusPoolProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy bonus pool proposal.</summary>
public sealed record LegacyBonusPoolProposalValidator(List<ILegacyBonusPoolProposalRule> Rules)
{
    public List<string> Validate(LegacyBonusPoolProposal item)
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

    public void RequireValid(LegacyBonusPoolProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyBonusPoolProposal> items)
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

/// <summary>Functions of legacy bonus pool proposal rules.</summary>
public static class LegacyBonusPoolProposalRules
{
    public static LegacyBonusPoolProposalValidator DefaultLegacyBonusPoolProposalValidator()
    {
        return new LegacyBonusPoolProposalValidator(
            new List<ILegacyBonusPoolProposalRule>
            {
                new LegacyBonusPoolProposalRequiredCodeRule("legacy bonus pool proposal needs an entry"),
                new LegacyBonusPoolProposalBadgeLengthRule(86400),
                new LegacyBonusPoolProposalLimitRangeRule(120, 86400),
                new LegacyBonusPoolProposalRetriesRangeRule(1000, 86400),
                new LegacyBonusPoolProposalAllowedLabelRule(new List<string> { "regional" }),
            });
    }
}
