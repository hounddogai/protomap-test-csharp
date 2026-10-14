namespace Acme.Workforce.Compliance.Audit.Mapping;

/// <summary>A rule a audit template must pass. Check returns its problems, or none.</summary>
public interface IAuditTemplateRule
{
    List<string> Check(AuditTemplate item);
}

public sealed record AuditTemplateApprovedGuardRule(int Limit) : IAuditTemplateRule
{
    public List<string> Check(AuditTemplate item)
    {
        if (item.Approved && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record AuditTemplateCodePrefixRule(string Prefix) : IAuditTemplateRule
{
    public List<string> Check(AuditTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AuditTemplateQuantityRangeRule(int Min, int Max) : IAuditTemplateRule
{
    public List<string> Check(AuditTemplate item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit template.</summary>
public sealed record AuditTemplateValidator(List<IAuditTemplateRule> Rules)
{
    public List<string> Validate(AuditTemplate item)
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

    public void RequireValid(AuditTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditTemplate> items)
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

/// <summary>Functions of audit template rules.</summary>
public static class AuditTemplateRules
{
    public static AuditTemplateValidator DefaultAuditTemplateValidator()
    {
        return new AuditTemplateValidator(
            new List<IAuditTemplateRule>
            {
                new AuditTemplateApprovedGuardRule(80),
                new AuditTemplateCodePrefixRule("ACM"),
                new AuditTemplateQuantityRangeRule(64, 255),
            });
    }
}
