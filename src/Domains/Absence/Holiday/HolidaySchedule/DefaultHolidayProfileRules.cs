namespace Acme.Workforce.Absence.Holiday.HolidaySchedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default holiday profile must pass. Check returns its problems, or none.</summary>
public interface IDefaultHolidayProfileRule
{
    List<string> Check(DefaultHolidayProfile item);
}

public sealed record DefaultHolidayProfileRequiredLabelRule(string Message) : IDefaultHolidayProfileRule
{
    public List<string> Check(DefaultHolidayProfile item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultHolidayProfileTitleLengthRule(int MaxLength) : IDefaultHolidayProfileRule
{
    public List<string> Check(DefaultHolidayProfile item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHolidayProfileCodeLengthRule(int MaxLength) : IDefaultHolidayProfileRule
{
    public List<string> Check(DefaultHolidayProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHolidayProfileApprovedGuardRule(int Limit) : IDefaultHolidayProfileRule
{
    public List<string> Check(DefaultHolidayProfile item)
    {
        if (item.Approved && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHolidayProfileAllowedLabelRule(List<string> Allowed) : IDefaultHolidayProfileRule
{
    public List<string> Check(DefaultHolidayProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHolidayProfileCodePrefixRule(string Prefix) : IDefaultHolidayProfileRule
{
    public List<string> Check(DefaultHolidayProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default holiday profile.</summary>
public sealed record DefaultHolidayProfileValidator(List<IDefaultHolidayProfileRule> Rules)
{
    public List<string> Validate(DefaultHolidayProfile item)
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

    public void RequireValid(DefaultHolidayProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultHolidayProfile> items)
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

/// <summary>Functions of default holiday profile rules.</summary>
public static class DefaultHolidayProfileRules
{
    public static DefaultHolidayProfileValidator DefaultDefaultHolidayProfileValidator()
    {
        return new DefaultHolidayProfileValidator(
            new List<IDefaultHolidayProfileRule>
            {
                new DefaultHolidayProfileRequiredLabelRule("default holiday profile needs this field"),
                new DefaultHolidayProfileTitleLengthRule(86400),
                new DefaultHolidayProfileCodeLengthRule(86400),
                new DefaultHolidayProfileApprovedGuardRule(120),
                new DefaultHolidayProfileAllowedLabelRule(new List<string> { "default", "tiered" }),
                new DefaultHolidayProfileCodePrefixRule("Q"),
            });
    }
}
