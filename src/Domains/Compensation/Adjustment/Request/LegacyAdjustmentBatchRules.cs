namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy adjustment batch must pass. Check returns its problems, or none.</summary>
public interface ILegacyAdjustmentBatchRule
{
    List<string> Check(LegacyAdjustmentBatch item);
}

public sealed record LegacyAdjustmentBatchUnitLengthRule(int MaxLength) : ILegacyAdjustmentBatchRule
{
    public List<string> Check(LegacyAdjustmentBatch item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAdjustmentBatchCodePrefixRule(string Prefix) : ILegacyAdjustmentBatchRule
{
    public List<string> Check(LegacyAdjustmentBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAdjustmentBatchCeilingRangeRule(double Min, double Max) : ILegacyAdjustmentBatchRule
{
    public List<string> Check(LegacyAdjustmentBatch item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyAdjustmentBatchRequiredCodeRule(string Message) : ILegacyAdjustmentBatchRule
{
    public List<string> Check(LegacyAdjustmentBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyAdjustmentBatchAllowedTeamRule(List<string> Allowed) : ILegacyAdjustmentBatchRule
{
    public List<string> Check(LegacyAdjustmentBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy adjustment batch.</summary>
public sealed record LegacyAdjustmentBatchValidator(List<ILegacyAdjustmentBatchRule> Rules)
{
    public List<string> Validate(LegacyAdjustmentBatch item)
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

    public void RequireValid(LegacyAdjustmentBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyAdjustmentBatch> items)
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

/// <summary>Functions of legacy adjustment batch rules.</summary>
public static class LegacyAdjustmentBatchRules
{
    public static LegacyAdjustmentBatchValidator DefaultLegacyAdjustmentBatchValidator()
    {
        return new LegacyAdjustmentBatchValidator(
            new List<ILegacyAdjustmentBatchRule>
            {
                new LegacyAdjustmentBatchUnitLengthRule(16),
                new LegacyAdjustmentBatchCodePrefixRule("ACM"),
                new LegacyAdjustmentBatchCeilingRangeRule(0.0, 1014.0),
                new LegacyAdjustmentBatchRequiredCodeRule("legacy adjustment batch needs this field"),
                new LegacyAdjustmentBatchAllowedTeamRule(new List<string> { "weighted" }),
            });
    }
}
