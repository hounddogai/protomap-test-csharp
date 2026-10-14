namespace Acme.Workforce.Contractors.Rate.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rate preference must pass. Check returns its problems, or none.</summary>
public interface IRatePreferenceRule
{
    List<string> Check(RatePreference item);
}

public sealed record RatePreferenceDepartmentLengthRule(int MaxLength) : IRatePreferenceRule
{
    public List<string> Check(RatePreference item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RatePreferenceAllowedDepartmentRule(List<string> Allowed) : IRatePreferenceRule
{
    public List<string> Check(RatePreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record RatePreferenceCodeLengthRule(int MaxLength) : IRatePreferenceRule
{
    public List<string> Check(RatePreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RatePreferenceRequiredDepartmentRule(string Message) : IRatePreferenceRule
{
    public List<string> Check(RatePreference item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RatePreferenceGraceDaysRangeRule(int Min, int Max) : IRatePreferenceRule
{
    public List<string> Check(RatePreference item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate preference.</summary>
public sealed record RatePreferenceValidator(List<IRatePreferenceRule> Rules)
{
    public List<string> Validate(RatePreference item)
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

    public void RequireValid(RatePreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RatePreference> items)
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

/// <summary>Functions of rate preference rules.</summary>
public static class RatePreferenceRules
{
    public static RatePreferenceValidator DefaultRatePreferenceValidator()
    {
        return new RatePreferenceValidator(
            new List<IRatePreferenceRule>
            {
                new RatePreferenceDepartmentLengthRule(80),
                new RatePreferenceAllowedDepartmentRule(new List<string> { "scheduled", "rolling" }),
                new RatePreferenceCodeLengthRule(80),
                new RatePreferenceRequiredDepartmentRule("rate preference needs a value"),
                new RatePreferenceGraceDaysRangeRule(16, 86400),
            });
    }
}
