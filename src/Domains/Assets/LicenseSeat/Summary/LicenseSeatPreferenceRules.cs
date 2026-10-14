namespace Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>A rule a license seat preference must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatPreferenceRule
{
    List<string> Check(LicenseSeatPreference item);
}

public sealed record LicenseSeatPreferenceChecksumLengthRule(int MaxLength) : ILicenseSeatPreferenceRule
{
    public List<string> Check(LicenseSeatPreference item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatPreferenceVisibleGuardRule(double Limit) : ILicenseSeatPreferenceRule
{
    public List<string> Check(LicenseSeatPreference item)
    {
        if (item.Visible && item.Hours > Limit)
        {
            return new List<string> { $"hours {item.Hours} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatPreferenceCodeLengthRule(int MaxLength) : ILicenseSeatPreferenceRule
{
    public List<string> Check(LicenseSeatPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat preference.</summary>
public sealed record LicenseSeatPreferenceValidator(List<ILicenseSeatPreferenceRule> Rules)
{
    public List<string> Validate(LicenseSeatPreference item)
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

    public void RequireValid(LicenseSeatPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatPreference> items)
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

/// <summary>Functions of license seat preference rules.</summary>
public static class LicenseSeatPreferenceRules
{
    public static LicenseSeatPreferenceValidator DefaultLicenseSeatPreferenceValidator()
    {
        return new LicenseSeatPreferenceValidator(
            new List<ILicenseSeatPreferenceRule>
            {
                new LicenseSeatPreferenceChecksumLengthRule(64),
                new LicenseSeatPreferenceVisibleGuardRule(6.0),
                new LicenseSeatPreferenceCodeLengthRule(80),
            });
    }
}
