namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy audit checkpoint must pass. Check returns its problems, or none.</summary>
public interface ILegacyAuditCheckpointRule
{
    List<string> Check(LegacyAuditCheckpoint item);
}

public sealed record LegacyAuditCheckpointAllowedTimezoneRule(List<string> Allowed) : ILegacyAuditCheckpointRule
{
    public List<string> Check(LegacyAuditCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAuditCheckpointRequiredTitleRule(string Message) : ILegacyAuditCheckpointRule
{
    public List<string> Check(LegacyAuditCheckpoint item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyAuditCheckpointCodePrefixRule(string Prefix) : ILegacyAuditCheckpointRule
{
    public List<string> Check(LegacyAuditCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAuditCheckpointQuantityRangeRule(int Min, int Max) : ILegacyAuditCheckpointRule
{
    public List<string> Check(LegacyAuditCheckpoint item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy audit checkpoint.</summary>
public sealed record LegacyAuditCheckpointValidator(List<ILegacyAuditCheckpointRule> Rules)
{
    public List<string> Validate(LegacyAuditCheckpoint item)
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

    public void RequireValid(LegacyAuditCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyAuditCheckpoint> items)
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

/// <summary>Functions of legacy audit checkpoint rules.</summary>
public static class LegacyAuditCheckpointRules
{
    public static LegacyAuditCheckpointValidator DefaultLegacyAuditCheckpointValidator()
    {
        return new LegacyAuditCheckpointValidator(
            new List<ILegacyAuditCheckpointRule>
            {
                new LegacyAuditCheckpointAllowedTimezoneRule(new List<string> { "global", "manual" }),
                new LegacyAuditCheckpointRequiredTitleRule("legacy audit checkpoint needs this field"),
                new LegacyAuditCheckpointCodePrefixRule("ACM"),
                new LegacyAuditCheckpointQuantityRangeRule(16, 1000),
            });
    }
}
