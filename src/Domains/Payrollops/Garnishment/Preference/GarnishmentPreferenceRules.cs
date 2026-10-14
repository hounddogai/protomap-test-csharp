namespace Acme.Workforce.Payrollops.Garnishment.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment preference must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentPreferenceRule
{
    List<string> Check(GarnishmentPreference item);
}

public sealed record GarnishmentPreferenceAllowedJobCodeRule(List<string> Allowed) : IGarnishmentPreferenceRule
{
    public List<string> Check(GarnishmentPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentPreferenceLimitRangeRule(int Min, int Max) : IGarnishmentPreferenceRule
{
    public List<string> Check(GarnishmentPreference item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GarnishmentPreferenceCodePrefixRule(string Prefix) : IGarnishmentPreferenceRule
{
    public List<string> Check(GarnishmentPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment preference.</summary>
public sealed record GarnishmentPreferenceValidator(List<IGarnishmentPreferenceRule> Rules)
{
    public List<string> Validate(GarnishmentPreference item)
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

    public void RequireValid(GarnishmentPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentPreference> items)
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

/// <summary>Functions of garnishment preference rules.</summary>
public static class GarnishmentPreferenceRules
{
    public static GarnishmentPreferenceValidator DefaultGarnishmentPreferenceValidator()
    {
        return new GarnishmentPreferenceValidator(
            new List<IGarnishmentPreferenceRule>
            {
                new GarnishmentPreferenceAllowedJobCodeRule(new List<string> { "primary" }),
                new GarnishmentPreferenceLimitRangeRule(32, 1000),
                new GarnishmentPreferenceCodePrefixRule("Q"),
            });
    }
}
