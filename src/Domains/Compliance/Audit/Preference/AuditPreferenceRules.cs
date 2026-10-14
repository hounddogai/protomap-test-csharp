namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a audit preference must pass. Check returns its problems, or none.</summary>
public interface IAuditPreferenceRule
{
    List<string> Check(AuditPreference item);
}

public sealed record AuditPreferenceRequiredCodeRule(string Message) : IAuditPreferenceRule
{
    public List<string> Check(AuditPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditPreferenceCodeLengthRule(int MaxLength) : IAuditPreferenceRule
{
    public List<string> Check(AuditPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditPreferenceAllowedLabelRule(List<string> Allowed) : IAuditPreferenceRule
{
    public List<string> Check(AuditPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit preference.</summary>
public sealed record AuditPreferenceValidator(List<IAuditPreferenceRule> Rules)
{
    public List<string> Validate(AuditPreference item)
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

    public void RequireValid(AuditPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditPreference> items)
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

/// <summary>Functions of audit preference rules.</summary>
public static class AuditPreferenceRules
{
    public static AuditPreferenceValidator DefaultAuditPreferenceValidator()
    {
        return new AuditPreferenceValidator(
            new List<IAuditPreferenceRule>
            {
                new AuditPreferenceRequiredCodeRule("audit preference needs a value"),
                new AuditPreferenceCodeLengthRule(16),
                new AuditPreferenceAllowedLabelRule(new List<string>()),
            });
    }
}
