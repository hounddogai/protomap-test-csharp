namespace Acme.Workforce.Compensation.BonusPool.Policy;

/// <summary>A rule a bonus pool variance must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolVarianceRule
{
    List<string> Check(BonusPoolVariance item);
}

public sealed record BonusPoolVarianceManagerIdLengthRule(int MaxLength) : IBonusPoolVarianceRule
{
    public List<string> Check(BonusPoolVariance item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolVarianceChannelLengthRule(int MaxLength) : IBonusPoolVarianceRule
{
    public List<string> Check(BonusPoolVariance item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolVarianceCodeLengthRule(int MaxLength) : IBonusPoolVarianceRule
{
    public List<string> Check(BonusPoolVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolVarianceRequiredLabelRule(string Message) : IBonusPoolVarianceRule
{
    public List<string> Check(BonusPoolVariance item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolVarianceCodePrefixRule(string Prefix) : IBonusPoolVarianceRule
{
    public List<string> Check(BonusPoolVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool variance.</summary>
public sealed record BonusPoolVarianceValidator(List<IBonusPoolVarianceRule> Rules)
{
    public List<string> Validate(BonusPoolVariance item)
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

    public void RequireValid(BonusPoolVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolVariance> items)
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

/// <summary>Functions of bonus pool variance rules.</summary>
public static class BonusPoolVarianceRules
{
    public static BonusPoolVarianceValidator DefaultBonusPoolVarianceValidator()
    {
        return new BonusPoolVarianceValidator(
            new List<IBonusPoolVarianceRule>
            {
                new BonusPoolVarianceManagerIdLengthRule(64),
                new BonusPoolVarianceChannelLengthRule(120),
                new BonusPoolVarianceCodeLengthRule(1000),
                new BonusPoolVarianceRequiredLabelRule("bonus pool variance needs an entry"),
                new BonusPoolVarianceCodePrefixRule("ACM"),
            });
    }
}
