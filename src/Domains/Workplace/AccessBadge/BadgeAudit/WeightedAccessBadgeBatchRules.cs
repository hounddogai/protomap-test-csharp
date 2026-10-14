namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

/// <summary>A rule a weighted access badge batch must pass. Check returns its problems, or none.</summary>
public interface IWeightedAccessBadgeBatchRule
{
    List<string> Check(WeightedAccessBadgeBatch item);
}

public sealed record WeightedAccessBadgeBatchLabelLengthRule(int MaxLength) : IWeightedAccessBadgeBatchRule
{
    public List<string> Check(WeightedAccessBadgeBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccessBadgeBatchRequiredChecksumRule(string Message) : IWeightedAccessBadgeBatchRule
{
    public List<string> Check(WeightedAccessBadgeBatch item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccessBadgeBatchCodePrefixRule(string Prefix) : IWeightedAccessBadgeBatchRule
{
    public List<string> Check(WeightedAccessBadgeBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccessBadgeBatchCapacityRangeRule(int Min, int Max) : IWeightedAccessBadgeBatchRule
{
    public List<string> Check(WeightedAccessBadgeBatch item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccessBadgeBatchArchivedGuardRule(int Limit) : IWeightedAccessBadgeBatchRule
{
    public List<string> Check(WeightedAccessBadgeBatch item)
    {
        if (item.Archived && item.GraceDays > Limit)
        {
            return new List<string> { $"grace days {item.GraceDays} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted access badge batch.</summary>
public sealed record WeightedAccessBadgeBatchValidator(List<IWeightedAccessBadgeBatchRule> Rules)
{
    public List<string> Validate(WeightedAccessBadgeBatch item)
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

    public void RequireValid(WeightedAccessBadgeBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedAccessBadgeBatch> items)
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

/// <summary>Functions of weighted access badge batch rules.</summary>
public static class WeightedAccessBadgeBatchRules
{
    public static WeightedAccessBadgeBatchValidator DefaultWeightedAccessBadgeBatchValidator()
    {
        return new WeightedAccessBadgeBatchValidator(
            new List<IWeightedAccessBadgeBatchRule>
            {
                new WeightedAccessBadgeBatchLabelLengthRule(80),
                new WeightedAccessBadgeBatchRequiredChecksumRule("weighted access badge batch needs this field"),
                new WeightedAccessBadgeBatchCodePrefixRule("X"),
                new WeightedAccessBadgeBatchCapacityRangeRule(80, 86400),
                new WeightedAccessBadgeBatchArchivedGuardRule(1000),
            });
    }
}
