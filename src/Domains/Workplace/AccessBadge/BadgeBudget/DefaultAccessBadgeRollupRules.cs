namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>A rule a default access badge rollup must pass. Check returns its problems, or none.</summary>
public interface IDefaultAccessBadgeRollupRule
{
    List<string> Check(DefaultAccessBadgeRollup item);
}

public sealed record DefaultAccessBadgeRollupQuantityRangeRule(int Min, int Max) : IDefaultAccessBadgeRollupRule
{
    public List<string> Check(DefaultAccessBadgeRollup item)
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

public sealed record DefaultAccessBadgeRollupWindowDaysRangeRule(int Min, int Max) : IDefaultAccessBadgeRollupRule
{
    public List<string> Check(DefaultAccessBadgeRollup item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAccessBadgeRollupRequiredLabelRule(string Message) : IDefaultAccessBadgeRollupRule
{
    public List<string> Check(DefaultAccessBadgeRollup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultAccessBadgeRollupCodePrefixRule(string Prefix) : IDefaultAccessBadgeRollupRule
{
    public List<string> Check(DefaultAccessBadgeRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAccessBadgeRollupLabelLengthRule(int MaxLength) : IDefaultAccessBadgeRollupRule
{
    public List<string> Check(DefaultAccessBadgeRollup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAccessBadgeRollupPinnedGuardRule(int Limit) : IDefaultAccessBadgeRollupRule
{
    public List<string> Check(DefaultAccessBadgeRollup item)
    {
        if (item.Pinned && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default access badge rollup.</summary>
public sealed record DefaultAccessBadgeRollupValidator(List<IDefaultAccessBadgeRollupRule> Rules)
{
    public List<string> Validate(DefaultAccessBadgeRollup item)
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

    public void RequireValid(DefaultAccessBadgeRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultAccessBadgeRollup> items)
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

/// <summary>Functions of default access badge rollup rules.</summary>
public static class DefaultAccessBadgeRollupRules
{
    public static DefaultAccessBadgeRollupValidator DefaultDefaultAccessBadgeRollupValidator()
    {
        return new DefaultAccessBadgeRollupValidator(
            new List<IDefaultAccessBadgeRollupRule>
            {
                new DefaultAccessBadgeRollupQuantityRangeRule(32, 255),
                new DefaultAccessBadgeRollupWindowDaysRangeRule(16, 64),
                new DefaultAccessBadgeRollupRequiredLabelRule("default access badge rollup needs a value"),
                new DefaultAccessBadgeRollupCodePrefixRule("REF"),
                new DefaultAccessBadgeRollupLabelLengthRule(16),
                new DefaultAccessBadgeRollupPinnedGuardRule(86400),
            });
    }
}
