namespace Acme.Workforce.Talent.CareerPath.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a career path preference must pass. Check returns its problems, or none.</summary>
public interface ICareerPathPreferenceRule
{
    List<string> Check(CareerPathPreference item);
}

public sealed record CareerPathPreferenceRequiredSalaryBandRule(string Message) : ICareerPathPreferenceRule
{
    public List<string> Check(CareerPathPreference item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathPreferenceRequiredCodeRule(string Message) : ICareerPathPreferenceRule
{
    public List<string> Check(CareerPathPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathPreferenceRequiredTitleRule(string Message) : ICareerPathPreferenceRule
{
    public List<string> Check(CareerPathPreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathPreferenceArchivedGuardRule(int Limit) : ICareerPathPreferenceRule
{
    public List<string> Check(CareerPathPreference item)
    {
        if (item.Archived && item.DurationMinutes > Limit)
        {
            return new List<string> { $"duration minutes {item.DurationMinutes} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathPreferenceAllowedTimezoneRule(List<string> Allowed) : ICareerPathPreferenceRule
{
    public List<string> Check(CareerPathPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path preference.</summary>
public sealed record CareerPathPreferenceValidator(List<ICareerPathPreferenceRule> Rules)
{
    public List<string> Validate(CareerPathPreference item)
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

    public void RequireValid(CareerPathPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathPreference> items)
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

/// <summary>Functions of career path preference rules.</summary>
public static class CareerPathPreferenceRules
{
    public static CareerPathPreferenceValidator DefaultCareerPathPreferenceValidator()
    {
        return new CareerPathPreferenceValidator(
            new List<ICareerPathPreferenceRule>
            {
                new CareerPathPreferenceRequiredSalaryBandRule("career path preference needs this field"),
                new CareerPathPreferenceRequiredCodeRule("career path preference needs this field"),
                new CareerPathPreferenceRequiredTitleRule("career path preference needs a value"),
                new CareerPathPreferenceArchivedGuardRule(80),
                new CareerPathPreferenceAllowedTimezoneRule(new List<string> { "scheduled" }),
            });
    }
}
