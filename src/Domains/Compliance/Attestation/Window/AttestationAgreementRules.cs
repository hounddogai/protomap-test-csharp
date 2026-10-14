namespace Acme.Workforce.Compliance.Attestation.Window;

/// <summary>A rule a attestation agreement must pass. Check returns its problems, or none.</summary>
public interface IAttestationAgreementRule
{
    List<string> Check(AttestationAgreement item);
}

public sealed record AttestationAgreementEndsAtRangeRule(int Min, int Max) : IAttestationAgreementRule
{
    public List<string> Check(AttestationAgreement item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationAgreementRequiredCategoryRule(string Message) : IAttestationAgreementRule
{
    public List<string> Check(AttestationAgreement item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationAgreementCreatedAtRangeRule(int Min, int Max) : IAttestationAgreementRule
{
    public List<string> Check(AttestationAgreement item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationAgreementFloorValueRangeRule(double Min, double Max) : IAttestationAgreementRule
{
    public List<string> Check(AttestationAgreement item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation agreement.</summary>
public sealed record AttestationAgreementValidator(List<IAttestationAgreementRule> Rules)
{
    public List<string> Validate(AttestationAgreement item)
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

    public void RequireValid(AttestationAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationAgreement> items)
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

/// <summary>Functions of attestation agreement rules.</summary>
public static class AttestationAgreementRules
{
    public static AttestationAgreementValidator DefaultAttestationAgreementValidator()
    {
        return new AttestationAgreementValidator(
            new List<IAttestationAgreementRule>
            {
                new AttestationAgreementEndsAtRangeRule(1000, 86400),
                new AttestationAgreementRequiredCategoryRule("attestation agreement needs an entry"),
                new AttestationAgreementCreatedAtRangeRule(80, 1000),
                new AttestationAgreementFloorValueRangeRule(3.4, 516.1),
            });
    }
}
