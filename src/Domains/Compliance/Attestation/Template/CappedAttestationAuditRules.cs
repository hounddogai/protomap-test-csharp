namespace Acme.Workforce.Compliance.Attestation.Template;

/// <summary>A rule a capped attestation audit must pass. Check returns its problems, or none.</summary>
public interface ICappedAttestationAuditRule
{
    List<string> Check(CappedAttestationAudit item);
}

public sealed record CappedAttestationAuditMaximumRangeRule(double Min, double Max) : ICappedAttestationAuditRule
{
    public List<string> Check(CappedAttestationAudit item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedAttestationAuditLabelLengthRule(int MaxLength) : ICappedAttestationAuditRule
{
    public List<string> Check(CappedAttestationAudit item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAttestationAuditRequiredDescriptionRule(string Message) : ICappedAttestationAuditRule
{
    public List<string> Check(CappedAttestationAudit item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped attestation audit.</summary>
public sealed record CappedAttestationAuditValidator(List<ICappedAttestationAuditRule> Rules)
{
    public List<string> Validate(CappedAttestationAudit item)
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

    public void RequireValid(CappedAttestationAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAttestationAudit> items)
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

/// <summary>Functions of capped attestation audit rules.</summary>
public static class CappedAttestationAuditRules
{
    public static CappedAttestationAuditValidator DefaultCappedAttestationAuditValidator()
    {
        return new CappedAttestationAuditValidator(
            new List<ICappedAttestationAuditRule>
            {
                new CappedAttestationAuditMaximumRangeRule(5.3, 4684.6),
                new CappedAttestationAuditLabelLengthRule(80),
                new CappedAttestationAuditRequiredDescriptionRule("capped attestation audit needs an entry"),
            });
    }
}
