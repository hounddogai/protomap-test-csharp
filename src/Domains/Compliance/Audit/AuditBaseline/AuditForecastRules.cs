namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>A rule a audit forecast must pass. Check returns its problems, or none.</summary>
public interface IAuditForecastRule
{
    List<string> Check(AuditForecast item);
}

public sealed record AuditForecastCodePrefixRule(string Prefix) : IAuditForecastRule
{
    public List<string> Check(AuditForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AuditForecastRequiredCodeRule(string Message) : IAuditForecastRule
{
    public List<string> Check(AuditForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditForecastUnitLengthRule(int MaxLength) : IAuditForecastRule
{
    public List<string> Check(AuditForecast item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditForecastRequiredUnitRule(string Message) : IAuditForecastRule
{
    public List<string> Check(AuditForecast item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit forecast.</summary>
public sealed record AuditForecastValidator(List<IAuditForecastRule> Rules)
{
    public List<string> Validate(AuditForecast item)
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

    public void RequireValid(AuditForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditForecast> items)
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

/// <summary>Functions of audit forecast rules.</summary>
public static class AuditForecastRules
{
    public static AuditForecastValidator DefaultAuditForecastValidator()
    {
        return new AuditForecastValidator(
            new List<IAuditForecastRule>
            {
                new AuditForecastCodePrefixRule("REF"),
                new AuditForecastRequiredCodeRule("audit forecast needs this field"),
                new AuditForecastUnitLengthRule(16),
                new AuditForecastRequiredUnitRule("audit forecast needs a value"),
            });
    }
}
