namespace Acme.Workforce.Absence.Holiday.Variance;

/// <summary>A rule a regional holiday adjustment must pass. Check returns its problems, or none.</summary>
public interface IRegionalHolidayAdjustmentRule
{
    List<string> Check(RegionalHolidayAdjustment item);
}

public sealed record RegionalHolidayAdjustmentCodePrefixRule(string Prefix) : IRegionalHolidayAdjustmentRule
{
    public List<string> Check(RegionalHolidayAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalHolidayAdjustmentRequiredPhoneRule(string Message) : IRegionalHolidayAdjustmentRule
{
    public List<string> Check(RegionalHolidayAdjustment item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalHolidayAdjustmentPhoneLengthRule(int MaxLength) : IRegionalHolidayAdjustmentRule
{
    public List<string> Check(RegionalHolidayAdjustment item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional holiday adjustment.</summary>
public sealed record RegionalHolidayAdjustmentValidator(List<IRegionalHolidayAdjustmentRule> Rules)
{
    public List<string> Validate(RegionalHolidayAdjustment item)
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

    public void RequireValid(RegionalHolidayAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalHolidayAdjustment> items)
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

/// <summary>Functions of regional holiday adjustment rules.</summary>
public static class RegionalHolidayAdjustmentRules
{
    public static RegionalHolidayAdjustmentValidator DefaultRegionalHolidayAdjustmentValidator()
    {
        return new RegionalHolidayAdjustmentValidator(
            new List<IRegionalHolidayAdjustmentRule>
            {
                new RegionalHolidayAdjustmentCodePrefixRule("X"),
                new RegionalHolidayAdjustmentRequiredPhoneRule("regional holiday adjustment needs this field"),
                new RegionalHolidayAdjustmentPhoneLengthRule(255),
            });
    }
}
