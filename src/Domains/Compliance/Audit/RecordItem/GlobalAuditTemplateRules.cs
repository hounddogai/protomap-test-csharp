namespace Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>A rule a global audit template must pass. Check returns its problems, or none.</summary>
public interface IGlobalAuditTemplateRule
{
    List<string> Check(GlobalAuditTemplate item);
}

public sealed record GlobalAuditTemplateCodeLengthRule(int MaxLength) : IGlobalAuditTemplateRule
{
    public List<string> Check(GlobalAuditTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAuditTemplateRequiredLabelRule(string Message) : IGlobalAuditTemplateRule
{
    public List<string> Check(GlobalAuditTemplate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAuditTemplateRequiredCodeRule(string Message) : IGlobalAuditTemplateRule
{
    public List<string> Check(GlobalAuditTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAuditTemplateOwnerIdLengthRule(int MaxLength) : IGlobalAuditTemplateRule
{
    public List<string> Check(GlobalAuditTemplate item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAuditTemplateLabelLengthRule(int MaxLength) : IGlobalAuditTemplateRule
{
    public List<string> Check(GlobalAuditTemplate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global audit template.</summary>
public sealed record GlobalAuditTemplateValidator(List<IGlobalAuditTemplateRule> Rules)
{
    public List<string> Validate(GlobalAuditTemplate item)
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

    public void RequireValid(GlobalAuditTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAuditTemplate> items)
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

/// <summary>Functions of global audit template rules.</summary>
public static class GlobalAuditTemplateRules
{
    public static GlobalAuditTemplateValidator DefaultGlobalAuditTemplateValidator()
    {
        return new GlobalAuditTemplateValidator(
            new List<IGlobalAuditTemplateRule>
            {
                new GlobalAuditTemplateCodeLengthRule(3600),
                new GlobalAuditTemplateRequiredLabelRule("global audit template needs a value"),
                new GlobalAuditTemplateRequiredCodeRule("global audit template needs this field"),
                new GlobalAuditTemplateOwnerIdLengthRule(16),
                new GlobalAuditTemplateLabelLengthRule(64),
            });
    }
}
