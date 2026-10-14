namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched audit template must pass. Check returns its problems, or none.</summary>
public interface IBatchedAuditTemplateRule
{
    List<string> Check(BatchedAuditTemplate item);
}

public sealed record BatchedAuditTemplateRequiredCodeRule(string Message) : IBatchedAuditTemplateRule
{
    public List<string> Check(BatchedAuditTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedAuditTemplateBudgetRangeRule(double Min, double Max) : IBatchedAuditTemplateRule
{
    public List<string> Check(BatchedAuditTemplate item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedAuditTemplateCodePrefixRule(string Prefix) : IBatchedAuditTemplateRule
{
    public List<string> Check(BatchedAuditTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAuditTemplateAllowedCodeRule(List<string> Allowed) : IBatchedAuditTemplateRule
{
    public List<string> Check(BatchedAuditTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAuditTemplateCeilingRangeRule(double Min, double Max) : IBatchedAuditTemplateRule
{
    public List<string> Check(BatchedAuditTemplate item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched audit template.</summary>
public sealed record BatchedAuditTemplateValidator(List<IBatchedAuditTemplateRule> Rules)
{
    public List<string> Validate(BatchedAuditTemplate item)
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

    public void RequireValid(BatchedAuditTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedAuditTemplate> items)
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

/// <summary>Functions of batched audit template rules.</summary>
public static class BatchedAuditTemplateRules
{
    public static BatchedAuditTemplateValidator DefaultBatchedAuditTemplateValidator()
    {
        return new BatchedAuditTemplateValidator(
            new List<IBatchedAuditTemplateRule>
            {
                new BatchedAuditTemplateRequiredCodeRule("batched audit template needs an entry"),
                new BatchedAuditTemplateBudgetRangeRule(2.2, 3011.3),
                new BatchedAuditTemplateCodePrefixRule("OPS"),
                new BatchedAuditTemplateAllowedCodeRule(new List<string> { "legacy", "scheduled" }),
                new BatchedAuditTemplateCeilingRangeRule(2.7, 3211.6),
            });
    }
}
