namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling access badge batch must pass. Check returns its problems, or none.</summary>
public interface IRollingAccessBadgeBatchRule
{
    List<string> Check(RollingAccessBadgeBatch item);
}

public sealed record RollingAccessBadgeBatchCodePrefixRule(string Prefix) : IRollingAccessBadgeBatchRule
{
    public List<string> Check(RollingAccessBadgeBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccessBadgeBatchTitleLengthRule(int MaxLength) : IRollingAccessBadgeBatchRule
{
    public List<string> Check(RollingAccessBadgeBatch item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccessBadgeBatchQuantityRangeRule(int Min, int Max) : IRollingAccessBadgeBatchRule
{
    public List<string> Check(RollingAccessBadgeBatch item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingAccessBadgeBatchAllowedCodeRule(List<string> Allowed) : IRollingAccessBadgeBatchRule
{
    public List<string> Check(RollingAccessBadgeBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling access badge batch.</summary>
public sealed record RollingAccessBadgeBatchValidator(List<IRollingAccessBadgeBatchRule> Rules)
{
    public List<string> Validate(RollingAccessBadgeBatch item)
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

    public void RequireValid(RollingAccessBadgeBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingAccessBadgeBatch> items)
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

/// <summary>Functions of rolling access badge batch rules.</summary>
public static class RollingAccessBadgeBatchRules
{
    public static RollingAccessBadgeBatchValidator DefaultRollingAccessBadgeBatchValidator()
    {
        return new RollingAccessBadgeBatchValidator(
            new List<IRollingAccessBadgeBatchRule>
            {
                new RollingAccessBadgeBatchCodePrefixRule("X"),
                new RollingAccessBadgeBatchTitleLengthRule(255),
                new RollingAccessBadgeBatchQuantityRangeRule(80, 1000),
                new RollingAccessBadgeBatchAllowedCodeRule(new List<string> { "adaptive", "global" }),
            });
    }
}
