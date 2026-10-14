namespace Acme.Workforce.Contractors.Rate.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual rate override must pass. Check returns its problems, or none.</summary>
public interface IManualRateOverrideRule
{
    List<string> Check(ManualRateOverride item);
}

public sealed record ManualRateOverrideRequiredLocaleRule(string Message) : IManualRateOverrideRule
{
    public List<string> Check(ManualRateOverride item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualRateOverrideCodePrefixRule(string Prefix) : IManualRateOverrideRule
{
    public List<string> Check(ManualRateOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualRateOverrideAllowedNameRule(List<string> Allowed) : IManualRateOverrideRule
{
    public List<string> Check(ManualRateOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record ManualRateOverrideGraceDaysRangeRule(int Min, int Max) : IManualRateOverrideRule
{
    public List<string> Check(ManualRateOverride item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualRateOverrideApprovedGuardRule(int Limit) : IManualRateOverrideRule
{
    public List<string> Check(ManualRateOverride item)
    {
        if (item.Approved && item.Sequence > Limit)
        {
            return new List<string> { $"sequence {item.Sequence} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record ManualRateOverrideRequiredExternalRefRule(string Message) : IManualRateOverrideRule
{
    public List<string> Check(ManualRateOverride item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual rate override.</summary>
public sealed record ManualRateOverrideValidator(List<IManualRateOverrideRule> Rules)
{
    public List<string> Validate(ManualRateOverride item)
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

    public void RequireValid(ManualRateOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualRateOverride> items)
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

/// <summary>Functions of manual rate override rules.</summary>
public static class ManualRateOverrideRules
{
    public static ManualRateOverrideValidator DefaultManualRateOverrideValidator()
    {
        return new ManualRateOverrideValidator(
            new List<IManualRateOverrideRule>
            {
                new ManualRateOverrideRequiredLocaleRule("manual rate override needs a value"),
                new ManualRateOverrideCodePrefixRule("X"),
                new ManualRateOverrideAllowedNameRule(new List<string> { "legacy", "seasonal", "regional" }),
                new ManualRateOverrideGraceDaysRangeRule(1000, 1000),
                new ManualRateOverrideApprovedGuardRule(1000),
                new ManualRateOverrideRequiredExternalRefRule("manual rate override needs a value"),
            });
    }
}
