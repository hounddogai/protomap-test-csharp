namespace Acme.Workforce.Compliance.Audit.AuditManifest;

/// <summary>A rule a primary audit mapping must pass. Check returns its problems, or none.</summary>
public interface IPrimaryAuditMappingRule
{
    List<string> Check(PrimaryAuditMapping item);
}

public sealed record PrimaryAuditMappingRequiredLocaleRule(string Message) : IPrimaryAuditMappingRule
{
    public List<string> Check(PrimaryAuditMapping item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAuditMappingRequiredCodeRule(string Message) : IPrimaryAuditMappingRule
{
    public List<string> Check(PrimaryAuditMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAuditMappingLabelLengthRule(int MaxLength) : IPrimaryAuditMappingRule
{
    public List<string> Check(PrimaryAuditMapping item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAuditMappingCodePrefixRule(string Prefix) : IPrimaryAuditMappingRule
{
    public List<string> Check(PrimaryAuditMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary audit mapping.</summary>
public sealed record PrimaryAuditMappingValidator(List<IPrimaryAuditMappingRule> Rules)
{
    public List<string> Validate(PrimaryAuditMapping item)
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

    public void RequireValid(PrimaryAuditMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryAuditMapping> items)
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

/// <summary>Functions of primary audit mapping rules.</summary>
public static class PrimaryAuditMappingRules
{
    public static PrimaryAuditMappingValidator DefaultPrimaryAuditMappingValidator()
    {
        return new PrimaryAuditMappingValidator(
            new List<IPrimaryAuditMappingRule>
            {
                new PrimaryAuditMappingRequiredLocaleRule("primary audit mapping needs a value"),
                new PrimaryAuditMappingRequiredCodeRule("primary audit mapping needs this field"),
                new PrimaryAuditMappingLabelLengthRule(120),
                new PrimaryAuditMappingCodePrefixRule("Q"),
            });
    }
}
