namespace Acme.Workforce.Compliance.Audit.Manifest;

/// <summary>A rule a audit manifest must pass. Check returns its problems, or none.</summary>
public interface IAuditManifestRule
{
    List<string> Check(AuditManifest item);
}

public sealed record AuditManifestRequiredCodeRule(string Message) : IAuditManifestRule
{
    public List<string> Check(AuditManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditManifestQuantityRangeRule(int Min, int Max) : IAuditManifestRule
{
    public List<string> Check(AuditManifest item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AuditManifestPhoneLengthRule(int MaxLength) : IAuditManifestRule
{
    public List<string> Check(AuditManifest item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditManifestTitleLengthRule(int MaxLength) : IAuditManifestRule
{
    public List<string> Check(AuditManifest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit manifest.</summary>
public sealed record AuditManifestValidator(List<IAuditManifestRule> Rules)
{
    public List<string> Validate(AuditManifest item)
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

    public void RequireValid(AuditManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditManifest> items)
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

/// <summary>Functions of audit manifest rules.</summary>
public static class AuditManifestRules
{
    public static AuditManifestValidator DefaultAuditManifestValidator()
    {
        return new AuditManifestValidator(
            new List<IAuditManifestRule>
            {
                new AuditManifestRequiredCodeRule("audit manifest needs a value"),
                new AuditManifestQuantityRangeRule(16, 32),
                new AuditManifestPhoneLengthRule(1000),
                new AuditManifestTitleLengthRule(255),
            });
    }
}
