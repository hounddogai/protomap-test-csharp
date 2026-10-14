namespace Acme.Workforce.Absence.Holiday.Review;

/// <summary>A rule a holiday bundle must pass. Check returns its problems, or none.</summary>
public interface IHolidayBundleRule
{
    List<string> Check(HolidayBundle item);
}

public sealed record HolidayBundleUnitLengthRule(int MaxLength) : IHolidayBundleRule
{
    public List<string> Check(HolidayBundle item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayBundleRequiredCountryRule(string Message) : IHolidayBundleRule
{
    public List<string> Check(HolidayBundle item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayBundleVisibleGuardRule(int Limit) : IHolidayBundleRule
{
    public List<string> Check(HolidayBundle item)
    {
        if (item.Visible && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record HolidayBundleCodePrefixRule(string Prefix) : IHolidayBundleRule
{
    public List<string> Check(HolidayBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday bundle.</summary>
public sealed record HolidayBundleValidator(List<IHolidayBundleRule> Rules)
{
    public List<string> Validate(HolidayBundle item)
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

    public void RequireValid(HolidayBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayBundle> items)
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

/// <summary>Functions of holiday bundle rules.</summary>
public static class HolidayBundleRules
{
    public static HolidayBundleValidator DefaultHolidayBundleValidator()
    {
        return new HolidayBundleValidator(
            new List<IHolidayBundleRule>
            {
                new HolidayBundleUnitLengthRule(80),
                new HolidayBundleRequiredCountryRule("holiday bundle needs this field"),
                new HolidayBundleVisibleGuardRule(1000),
                new HolidayBundleCodePrefixRule("REF"),
            });
    }
}
