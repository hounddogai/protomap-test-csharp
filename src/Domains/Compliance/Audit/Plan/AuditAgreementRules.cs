namespace Acme.Workforce.Compliance.Audit.Plan;

/// <summary>A rule a audit agreement must pass. Check returns its problems, or none.</summary>
public interface IAuditAgreementRule
{
    List<string> Check(AuditAgreement item);
}

public sealed record AuditAgreementCostRangeRule(double Min, double Max) : IAuditAgreementRule
{
    public List<string> Check(AuditAgreement item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AuditAgreementExpiresAtRangeRule(int Min, int Max) : IAuditAgreementRule
{
    public List<string> Check(AuditAgreement item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AuditAgreementCodeLengthRule(int MaxLength) : IAuditAgreementRule
{
    public List<string> Check(AuditAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit agreement.</summary>
public sealed record AuditAgreementValidator(List<IAuditAgreementRule> Rules)
{
    public List<string> Validate(AuditAgreement item)
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

    public void RequireValid(AuditAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditAgreement> items)
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

/// <summary>Functions of audit agreement rules.</summary>
public static class AuditAgreementRules
{
    public static AuditAgreementValidator DefaultAuditAgreementValidator()
    {
        return new AuditAgreementValidator(
            new List<IAuditAgreementRule>
            {
                new AuditAgreementCostRangeRule(9.6, 1546.5),
                new AuditAgreementExpiresAtRangeRule(64, 3600),
                new AuditAgreementCodeLengthRule(120),
            });
    }
}
