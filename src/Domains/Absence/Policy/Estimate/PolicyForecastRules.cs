namespace Acme.Workforce.Absence.Policy.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy forecast must pass. Check returns its problems, or none.</summary>
public interface IPolicyForecastRule
{
    List<string> Check(PolicyForecast item);
}

public sealed record PolicyForecastRequiredSummaryRule(string Message) : IPolicyForecastRule
{
    public List<string> Check(PolicyForecast item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyForecastCodePrefixRule(string Prefix) : IPolicyForecastRule
{
    public List<string> Check(PolicyForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyForecastRequiredCodeRule(string Message) : IPolicyForecastRule
{
    public List<string> Check(PolicyForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyForecastAllowedTitleRule(List<string> Allowed) : IPolicyForecastRule
{
    public List<string> Check(PolicyForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy forecast.</summary>
public sealed record PolicyForecastValidator(List<IPolicyForecastRule> Rules)
{
    public List<string> Validate(PolicyForecast item)
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

    public void RequireValid(PolicyForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyForecast> items)
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

/// <summary>Functions of policy forecast rules.</summary>
public static class PolicyForecastRules
{
    public static PolicyForecastValidator DefaultPolicyForecastValidator()
    {
        return new PolicyForecastValidator(
            new List<IPolicyForecastRule>
            {
                new PolicyForecastRequiredSummaryRule("policy forecast needs this field"),
                new PolicyForecastCodePrefixRule("Q"),
                new PolicyForecastRequiredCodeRule("policy forecast needs this field"),
                new PolicyForecastAllowedTitleRule(new List<string> { "manual", "weighted" }),
            });
    }
}
