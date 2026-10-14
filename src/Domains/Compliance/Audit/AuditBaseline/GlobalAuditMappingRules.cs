namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global audit mapping must pass. Check returns its problems, or none.</summary>
public interface IGlobalAuditMappingRule
{
    List<string> Check(GlobalAuditMapping item);
}

public sealed record GlobalAuditMappingAllowedCodeRule(List<string> Allowed) : IGlobalAuditMappingRule
{
    public List<string> Check(GlobalAuditMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAuditMappingNameLengthRule(int MaxLength) : IGlobalAuditMappingRule
{
    public List<string> Check(GlobalAuditMapping item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAuditMappingRequiredNameRule(string Message) : IGlobalAuditMappingRule
{
    public List<string> Check(GlobalAuditMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAuditMappingRequiredCodeRule(string Message) : IGlobalAuditMappingRule
{
    public List<string> Check(GlobalAuditMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global audit mapping.</summary>
public sealed record GlobalAuditMappingValidator(List<IGlobalAuditMappingRule> Rules)
{
    public List<string> Validate(GlobalAuditMapping item)
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

    public void RequireValid(GlobalAuditMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAuditMapping> items)
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

/// <summary>Functions of global audit mapping rules.</summary>
public static class GlobalAuditMappingRules
{
    public static GlobalAuditMappingValidator DefaultGlobalAuditMappingValidator()
    {
        return new GlobalAuditMappingValidator(
            new List<IGlobalAuditMappingRule>
            {
                new GlobalAuditMappingAllowedCodeRule(new List<string> { "lenient" }),
                new GlobalAuditMappingNameLengthRule(1000),
                new GlobalAuditMappingRequiredNameRule("global audit mapping needs a value"),
                new GlobalAuditMappingRequiredCodeRule("global audit mapping needs an entry"),
            });
    }
}
