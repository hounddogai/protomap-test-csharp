namespace Acme.Workforce.Absence.Holiday.Request;

/// <summary>A rule a default holiday rollup must pass. Check returns its problems, or none.</summary>
public interface IDefaultHolidayRollupRule
{
    List<string> Check(DefaultHolidayRollup item);
}

public sealed record DefaultHolidayRollupRequiredPhoneRule(string Message) : IDefaultHolidayRollupRule
{
    public List<string> Check(DefaultHolidayRollup item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultHolidayRollupDiscountRangeRule(double Min, double Max) : IDefaultHolidayRollupRule
{
    public List<string> Check(DefaultHolidayRollup item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHolidayRollupCodeLengthRule(int MaxLength) : IDefaultHolidayRollupRule
{
    public List<string> Check(DefaultHolidayRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHolidayRollupAttemptsRangeRule(int Min, int Max) : IDefaultHolidayRollupRule
{
    public List<string> Check(DefaultHolidayRollup item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHolidayRollupTagsLimitRule(int MaxCount, List<string> Banned) : IDefaultHolidayRollupRule
{
    public List<string> Check(DefaultHolidayRollup item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record DefaultHolidayRollupCodePrefixRule(string Prefix) : IDefaultHolidayRollupRule
{
    public List<string> Check(DefaultHolidayRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default holiday rollup.</summary>
public sealed record DefaultHolidayRollupValidator(List<IDefaultHolidayRollupRule> Rules)
{
    public List<string> Validate(DefaultHolidayRollup item)
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

    public void RequireValid(DefaultHolidayRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultHolidayRollup> items)
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

/// <summary>Functions of default holiday rollup rules.</summary>
public static class DefaultHolidayRollupRules
{
    public static DefaultHolidayRollupValidator DefaultDefaultHolidayRollupValidator()
    {
        return new DefaultHolidayRollupValidator(
            new List<IDefaultHolidayRollupRule>
            {
                new DefaultHolidayRollupRequiredPhoneRule("default holiday rollup needs a value"),
                new DefaultHolidayRollupDiscountRangeRule(3.1, 2152.6),
                new DefaultHolidayRollupCodeLengthRule(86400),
                new DefaultHolidayRollupAttemptsRangeRule(64, 64),
                new DefaultHolidayRollupTagsLimitRule(255, new List<string> { "strict", "tiered", "lenient" }),
                new DefaultHolidayRollupCodePrefixRule("ACM"),
            });
    }
}
