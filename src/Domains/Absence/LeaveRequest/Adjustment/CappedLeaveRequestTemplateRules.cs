namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped leave request template must pass. Check returns its problems, or none.</summary>
public interface ICappedLeaveRequestTemplateRule
{
    List<string> Check(CappedLeaveRequestTemplate item);
}

public sealed record CappedLeaveRequestTemplateRequiredDescriptionRule(string Message) : ICappedLeaveRequestTemplateRule
{
    public List<string> Check(CappedLeaveRequestTemplate item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedLeaveRequestTemplateRequiredDepartmentRule(string Message) : ICappedLeaveRequestTemplateRule
{
    public List<string> Check(CappedLeaveRequestTemplate item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedLeaveRequestTemplateAllowedCodeRule(List<string> Allowed) : ICappedLeaveRequestTemplateRule
{
    public List<string> Check(CappedLeaveRequestTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record CappedLeaveRequestTemplateCodePrefixRule(string Prefix) : ICappedLeaveRequestTemplateRule
{
    public List<string> Check(CappedLeaveRequestTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped leave request template.</summary>
public sealed record CappedLeaveRequestTemplateValidator(List<ICappedLeaveRequestTemplateRule> Rules)
{
    public List<string> Validate(CappedLeaveRequestTemplate item)
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

    public void RequireValid(CappedLeaveRequestTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedLeaveRequestTemplate> items)
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

/// <summary>Functions of capped leave request template rules.</summary>
public static class CappedLeaveRequestTemplateRules
{
    public static CappedLeaveRequestTemplateValidator DefaultCappedLeaveRequestTemplateValidator()
    {
        return new CappedLeaveRequestTemplateValidator(
            new List<ICappedLeaveRequestTemplateRule>
            {
                new CappedLeaveRequestTemplateRequiredDescriptionRule("capped leave request template needs an entry"),
                new CappedLeaveRequestTemplateRequiredDepartmentRule("capped leave request template needs an entry"),
                new CappedLeaveRequestTemplateAllowedCodeRule(new List<string>()),
                new CappedLeaveRequestTemplateCodePrefixRule("REF"),
            });
    }
}
