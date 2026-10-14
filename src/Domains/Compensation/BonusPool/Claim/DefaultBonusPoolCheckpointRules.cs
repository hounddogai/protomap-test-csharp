namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default bonus pool checkpoint must pass. Check returns its problems, or none.</summary>
public interface IDefaultBonusPoolCheckpointRule
{
    List<string> Check(DefaultBonusPoolCheckpoint item);
}

public sealed record DefaultBonusPoolCheckpointQuantityRangeRule(int Min, int Max) : IDefaultBonusPoolCheckpointRule
{
    public List<string> Check(DefaultBonusPoolCheckpoint item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultBonusPoolCheckpointAllowedCodeRule(List<string> Allowed) : IDefaultBonusPoolCheckpointRule
{
    public List<string> Check(DefaultBonusPoolCheckpoint item)
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

public sealed record DefaultBonusPoolCheckpointCodePrefixRule(string Prefix) : IDefaultBonusPoolCheckpointRule
{
    public List<string> Check(DefaultBonusPoolCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultBonusPoolCheckpointCodeLengthRule(int MaxLength) : IDefaultBonusPoolCheckpointRule
{
    public List<string> Check(DefaultBonusPoolCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default bonus pool checkpoint.</summary>
public sealed record DefaultBonusPoolCheckpointValidator(List<IDefaultBonusPoolCheckpointRule> Rules)
{
    public List<string> Validate(DefaultBonusPoolCheckpoint item)
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

    public void RequireValid(DefaultBonusPoolCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultBonusPoolCheckpoint> items)
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

/// <summary>Functions of default bonus pool checkpoint rules.</summary>
public static class DefaultBonusPoolCheckpointRules
{
    public static DefaultBonusPoolCheckpointValidator DefaultDefaultBonusPoolCheckpointValidator()
    {
        return new DefaultBonusPoolCheckpointValidator(
            new List<IDefaultBonusPoolCheckpointRule>
            {
                new DefaultBonusPoolCheckpointQuantityRangeRule(255, 255),
                new DefaultBonusPoolCheckpointAllowedCodeRule(new List<string> { "legacy" }),
                new DefaultBonusPoolCheckpointCodePrefixRule("REF"),
                new DefaultBonusPoolCheckpointCodeLengthRule(1000),
            });
    }
}
