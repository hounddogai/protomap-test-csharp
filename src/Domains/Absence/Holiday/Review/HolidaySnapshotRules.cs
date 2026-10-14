namespace Acme.Workforce.Absence.Holiday.Review;

/// <summary>A rule a holiday snapshot must pass. Check returns its problems, or none.</summary>
public interface IHolidaySnapshotRule
{
    List<string> Check(HolidaySnapshot item);
}

public sealed record HolidaySnapshotNameLengthRule(int MaxLength) : IHolidaySnapshotRule
{
    public List<string> Check(HolidaySnapshot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidaySnapshotCodePrefixRule(string Prefix) : IHolidaySnapshotRule
{
    public List<string> Check(HolidaySnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HolidaySnapshotCodeLengthRule(int MaxLength) : IHolidaySnapshotRule
{
    public List<string> Check(HolidaySnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidaySnapshotCountryLengthRule(int MaxLength) : IHolidaySnapshotRule
{
    public List<string> Check(HolidaySnapshot item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday snapshot.</summary>
public sealed record HolidaySnapshotValidator(List<IHolidaySnapshotRule> Rules)
{
    public List<string> Validate(HolidaySnapshot item)
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

    public void RequireValid(HolidaySnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidaySnapshot> items)
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

/// <summary>Functions of holiday snapshot rules.</summary>
public static class HolidaySnapshotRules
{
    public static HolidaySnapshotValidator DefaultHolidaySnapshotValidator()
    {
        return new HolidaySnapshotValidator(
            new List<IHolidaySnapshotRule>
            {
                new HolidaySnapshotNameLengthRule(86400),
                new HolidaySnapshotCodePrefixRule("ACM"),
                new HolidaySnapshotCodeLengthRule(86400),
                new HolidaySnapshotCountryLengthRule(80),
            });
    }
}
