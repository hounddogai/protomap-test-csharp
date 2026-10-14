namespace Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>A rule a engagement preference must pass. Check returns its problems, or none.</summary>
public interface IEngagementPreferenceRule
{
    List<string> Check(EngagementPreference item);
}

public sealed record EngagementPreferenceRequiredCodeRule(string Message) : IEngagementPreferenceRule
{
    public List<string> Check(EngagementPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementPreferenceRequiredNameRule(string Message) : IEngagementPreferenceRule
{
    public List<string> Check(EngagementPreference item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementPreferenceRequiredSalaryBandRule(string Message) : IEngagementPreferenceRule
{
    public List<string> Check(EngagementPreference item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement preference.</summary>
public sealed record EngagementPreferenceValidator(List<IEngagementPreferenceRule> Rules)
{
    public List<string> Validate(EngagementPreference item)
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

    public void RequireValid(EngagementPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementPreference> items)
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

/// <summary>Functions of engagement preference rules.</summary>
public static class EngagementPreferenceRules
{
    public static EngagementPreferenceValidator DefaultEngagementPreferenceValidator()
    {
        return new EngagementPreferenceValidator(
            new List<IEngagementPreferenceRule>
            {
                new EngagementPreferenceRequiredCodeRule("engagement preference needs this field"),
                new EngagementPreferenceRequiredNameRule("engagement preference needs a value"),
                new EngagementPreferenceRequiredSalaryBandRule("engagement preference needs an entry"),
            });
    }
}
