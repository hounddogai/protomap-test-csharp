namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary desk forecast must pass. Check returns its problems, or none.</summary>
public interface IPrimaryDeskForecastRule
{
    List<string> Check(PrimaryDeskForecast item);
}

public sealed record PrimaryDeskForecastRequiredCodeRule(string Message) : IPrimaryDeskForecastRule
{
    public List<string> Check(PrimaryDeskForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryDeskForecastAllowedLabelRule(List<string> Allowed) : IPrimaryDeskForecastRule
{
    public List<string> Check(PrimaryDeskForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryDeskForecastCodePrefixRule(string Prefix) : IPrimaryDeskForecastRule
{
    public List<string> Check(PrimaryDeskForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryDeskForecastRequiredGradeRule(string Message) : IPrimaryDeskForecastRule
{
    public List<string> Check(PrimaryDeskForecast item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary desk forecast.</summary>
public sealed record PrimaryDeskForecastValidator(List<IPrimaryDeskForecastRule> Rules)
{
    public List<string> Validate(PrimaryDeskForecast item)
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

    public void RequireValid(PrimaryDeskForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryDeskForecast> items)
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

/// <summary>Functions of primary desk forecast rules.</summary>
public static class PrimaryDeskForecastRules
{
    public static PrimaryDeskForecastValidator DefaultPrimaryDeskForecastValidator()
    {
        return new PrimaryDeskForecastValidator(
            new List<IPrimaryDeskForecastRule>
            {
                new PrimaryDeskForecastRequiredCodeRule("primary desk forecast needs this field"),
                new PrimaryDeskForecastAllowedLabelRule(new List<string> { "rolling", "fallback" }),
                new PrimaryDeskForecastCodePrefixRule("ACM"),
                new PrimaryDeskForecastRequiredGradeRule("primary desk forecast needs a value"),
            });
    }
}
