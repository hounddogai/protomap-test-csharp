namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a equity grant score must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantScoreRule
{
    List<string> Check(EquityGrantScore item);
}

public sealed record EquityGrantScoreRequiredExternalRefRule(string Message) : IEquityGrantScoreRule
{
    public List<string> Check(EquityGrantScore item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantScoreAllowedExternalRefRule(List<string> Allowed) : IEquityGrantScoreRule
{
    public List<string> Check(EquityGrantScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is expired" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantScoreRateRangeRule(double Min, double Max) : IEquityGrantScoreRule
{
    public List<string> Check(EquityGrantScore item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantScoreHoursRangeRule(double Min, double Max) : IEquityGrantScoreRule
{
    public List<string> Check(EquityGrantScore item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantScoreRequiredNameRule(string Message) : IEquityGrantScoreRule
{
    public List<string> Check(EquityGrantScore item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant score.</summary>
public sealed record EquityGrantScoreValidator(List<IEquityGrantScoreRule> Rules)
{
    public List<string> Validate(EquityGrantScore item)
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

    public void RequireValid(EquityGrantScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantScore> items)
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

/// <summary>Functions of equity grant score rules.</summary>
public static class EquityGrantScoreRules
{
    public static EquityGrantScoreValidator DefaultEquityGrantScoreValidator()
    {
        return new EquityGrantScoreValidator(
            new List<IEquityGrantScoreRule>
            {
                new EquityGrantScoreRequiredExternalRefRule("equity grant score needs this field"),
                new EquityGrantScoreAllowedExternalRefRule(new List<string> { "lenient", "scheduled" }),
                new EquityGrantScoreRateRangeRule(0.5, 2406.0),
                new EquityGrantScoreHoursRangeRule(7.2, 832.6),
                new EquityGrantScoreRequiredNameRule("equity grant score needs this field"),
            });
    }
}
