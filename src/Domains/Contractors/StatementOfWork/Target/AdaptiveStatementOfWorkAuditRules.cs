namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive statement of work audit must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveStatementOfWorkAuditRule
{
    List<string> Check(AdaptiveStatementOfWorkAudit item);
}

public sealed record AdaptiveStatementOfWorkAuditNameLengthRule(int MaxLength) : IAdaptiveStatementOfWorkAuditRule
{
    public List<string> Check(AdaptiveStatementOfWorkAudit item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveStatementOfWorkAuditAllowedLabelRule(List<string> Allowed) : IAdaptiveStatementOfWorkAuditRule
{
    public List<string> Check(AdaptiveStatementOfWorkAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveStatementOfWorkAuditLabelLengthRule(int MaxLength) : IAdaptiveStatementOfWorkAuditRule
{
    public List<string> Check(AdaptiveStatementOfWorkAudit item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveStatementOfWorkAuditHoursRangeRule(double Min, double Max) : IAdaptiveStatementOfWorkAuditRule
{
    public List<string> Check(AdaptiveStatementOfWorkAudit item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveStatementOfWorkAuditRequiredNameRule(string Message) : IAdaptiveStatementOfWorkAuditRule
{
    public List<string> Check(AdaptiveStatementOfWorkAudit item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveStatementOfWorkAuditCodePrefixRule(string Prefix) : IAdaptiveStatementOfWorkAuditRule
{
    public List<string> Check(AdaptiveStatementOfWorkAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive statement of work audit.</summary>
public sealed record AdaptiveStatementOfWorkAuditValidator(List<IAdaptiveStatementOfWorkAuditRule> Rules)
{
    public List<string> Validate(AdaptiveStatementOfWorkAudit item)
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

    public void RequireValid(AdaptiveStatementOfWorkAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveStatementOfWorkAudit> items)
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

/// <summary>Functions of adaptive statement of work audit rules.</summary>
public static class AdaptiveStatementOfWorkAuditRules
{
    public static AdaptiveStatementOfWorkAuditValidator DefaultAdaptiveStatementOfWorkAuditValidator()
    {
        return new AdaptiveStatementOfWorkAuditValidator(
            new List<IAdaptiveStatementOfWorkAuditRule>
            {
                new AdaptiveStatementOfWorkAuditNameLengthRule(64),
                new AdaptiveStatementOfWorkAuditAllowedLabelRule(new List<string> { "weighted", "manual" }),
                new AdaptiveStatementOfWorkAuditLabelLengthRule(120),
                new AdaptiveStatementOfWorkAuditHoursRangeRule(6.5, 1873.4),
                new AdaptiveStatementOfWorkAuditRequiredNameRule("adaptive statement of work audit needs a value"),
                new AdaptiveStatementOfWorkAuditCodePrefixRule("OPS"),
            });
    }
}
