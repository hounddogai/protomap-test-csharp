namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual pool tier must pass. Check returns its problems, or none.</summary>
public interface IManualPoolTierRule
{
    List<string> Check(ManualPoolTier item);
}

public sealed record ManualPoolTierNameLengthRule(int MaxLength) : IManualPoolTierRule
{
    public List<string> Check(ManualPoolTier item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualPoolTierRequiredNameRule(string Message) : IManualPoolTierRule
{
    public List<string> Check(ManualPoolTier item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualPoolTierVisibleGuardRule(double Limit) : IManualPoolTierRule
{
    public List<string> Check(ManualPoolTier item)
    {
        if (item.Visible && item.Maximum > Limit)
        {
            return new List<string> { $"maximum {item.Maximum} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record ManualPoolTierAllowedDescriptionRule(List<string> Allowed) : IManualPoolTierRule
{
    public List<string> Check(ManualPoolTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record ManualPoolTierMaximumRangeRule(double Min, double Max) : IManualPoolTierRule
{
    public List<string> Check(ManualPoolTier item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualPoolTierCodePrefixRule(string Prefix) : IManualPoolTierRule
{
    public List<string> Check(ManualPoolTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual pool tier.</summary>
public sealed record ManualPoolTierValidator(List<IManualPoolTierRule> Rules)
{
    public List<string> Validate(ManualPoolTier item)
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

    public void RequireValid(ManualPoolTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualPoolTier> items)
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

/// <summary>Functions of manual pool tier rules.</summary>
public static class ManualPoolTierRules
{
    public static ManualPoolTierValidator DefaultManualPoolTierValidator()
    {
        return new ManualPoolTierValidator(
            new List<IManualPoolTierRule>
            {
                new ManualPoolTierNameLengthRule(3600),
                new ManualPoolTierRequiredNameRule("manual pool tier needs an entry"),
                new ManualPoolTierVisibleGuardRule(4.7),
                new ManualPoolTierAllowedDescriptionRule(new List<string>()),
                new ManualPoolTierMaximumRangeRule(9.7, 4974.1),
                new ManualPoolTierCodePrefixRule("X"),
            });
    }
}
