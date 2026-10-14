namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional bonus pool override must pass. Check returns its problems, or none.</summary>
public interface IRegionalBonusPoolOverrideRule
{
    List<string> Check(RegionalBonusPoolOverride item);
}

public sealed record RegionalBonusPoolOverrideRetriesRangeRule(int Min, int Max) : IRegionalBonusPoolOverrideRule
{
    public List<string> Check(RegionalBonusPoolOverride item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalBonusPoolOverrideAllowedCurrencyRule(List<string> Allowed) : IRegionalBonusPoolOverrideRule
{
    public List<string> Check(RegionalBonusPoolOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBonusPoolOverrideCodeLengthRule(int MaxLength) : IRegionalBonusPoolOverrideRule
{
    public List<string> Check(RegionalBonusPoolOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBonusPoolOverrideDurationMinutesRangeRule(int Min, int Max) : IRegionalBonusPoolOverrideRule
{
    public List<string> Check(RegionalBonusPoolOverride item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalBonusPoolOverrideRequiredCurrencyRule(string Message) : IRegionalBonusPoolOverrideRule
{
    public List<string> Check(RegionalBonusPoolOverride item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalBonusPoolOverrideCodePrefixRule(string Prefix) : IRegionalBonusPoolOverrideRule
{
    public List<string> Check(RegionalBonusPoolOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional bonus pool override.</summary>
public sealed record RegionalBonusPoolOverrideValidator(List<IRegionalBonusPoolOverrideRule> Rules)
{
    public List<string> Validate(RegionalBonusPoolOverride item)
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

    public void RequireValid(RegionalBonusPoolOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalBonusPoolOverride> items)
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

/// <summary>Functions of regional bonus pool override rules.</summary>
public static class RegionalBonusPoolOverrideRules
{
    public static RegionalBonusPoolOverrideValidator DefaultRegionalBonusPoolOverrideValidator()
    {
        return new RegionalBonusPoolOverrideValidator(
            new List<IRegionalBonusPoolOverrideRule>
            {
                new RegionalBonusPoolOverrideRetriesRangeRule(3600, 3600),
                new RegionalBonusPoolOverrideAllowedCurrencyRule(new List<string> { "regional", "primary" }),
                new RegionalBonusPoolOverrideCodeLengthRule(255),
                new RegionalBonusPoolOverrideDurationMinutesRangeRule(32, 80),
                new RegionalBonusPoolOverrideRequiredCurrencyRule("regional bonus pool override needs this field"),
                new RegionalBonusPoolOverrideCodePrefixRule("ACM"),
            });
    }
}
