namespace Acme.Workforce.Compliance.Attestation.Quota;

/// <summary>A rule a attestation quota must pass. Check returns its problems, or none.</summary>
public interface IAttestationQuotaRule
{
    List<string> Check(AttestationQuota item);
}

public sealed record AttestationQuotaNameLengthRule(int MaxLength) : IAttestationQuotaRule
{
    public List<string> Check(AttestationQuota item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationQuotaMinimumRangeRule(double Min, double Max) : IAttestationQuotaRule
{
    public List<string> Check(AttestationQuota item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationQuotaTitleLengthRule(int MaxLength) : IAttestationQuotaRule
{
    public List<string> Check(AttestationQuota item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationQuotaRequiredSourceRule(string Message) : IAttestationQuotaRule
{
    public List<string> Check(AttestationQuota item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation quota.</summary>
public sealed record AttestationQuotaValidator(List<IAttestationQuotaRule> Rules)
{
    public List<string> Validate(AttestationQuota item)
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

    public void RequireValid(AttestationQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationQuota> items)
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

/// <summary>Functions of attestation quota rules.</summary>
public static class AttestationQuotaRules
{
    public static AttestationQuotaValidator DefaultAttestationQuotaValidator()
    {
        return new AttestationQuotaValidator(
            new List<IAttestationQuotaRule>
            {
                new AttestationQuotaNameLengthRule(120),
                new AttestationQuotaMinimumRangeRule(6.6, 4855.0),
                new AttestationQuotaTitleLengthRule(3600),
                new AttestationQuotaRequiredSourceRule("attestation quota needs this field"),
            });
    }
}
