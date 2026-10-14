namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

/// <summary>A rule a capped attestation quota must pass. Check returns its problems, or none.</summary>
public interface ICappedAttestationQuotaRule
{
    List<string> Check(CappedAttestationQuota item);
}

public sealed record CappedAttestationQuotaLabelLengthRule(int MaxLength) : ICappedAttestationQuotaRule
{
    public List<string> Check(CappedAttestationQuota item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAttestationQuotaSegmentLengthRule(int MaxLength) : ICappedAttestationQuotaRule
{
    public List<string> Check(CappedAttestationQuota item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAttestationQuotaSalaryBandLengthRule(int MaxLength) : ICappedAttestationQuotaRule
{
    public List<string> Check(CappedAttestationQuota item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAttestationQuotaCodeLengthRule(int MaxLength) : ICappedAttestationQuotaRule
{
    public List<string> Check(CappedAttestationQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAttestationQuotaThresholdRangeRule(double Min, double Max) : ICappedAttestationQuotaRule
{
    public List<string> Check(CappedAttestationQuota item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped attestation quota.</summary>
public sealed record CappedAttestationQuotaValidator(List<ICappedAttestationQuotaRule> Rules)
{
    public List<string> Validate(CappedAttestationQuota item)
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

    public void RequireValid(CappedAttestationQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAttestationQuota> items)
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

/// <summary>Functions of capped attestation quota rules.</summary>
public static class CappedAttestationQuotaRules
{
    public static CappedAttestationQuotaValidator DefaultCappedAttestationQuotaValidator()
    {
        return new CappedAttestationQuotaValidator(
            new List<ICappedAttestationQuotaRule>
            {
                new CappedAttestationQuotaLabelLengthRule(86400),
                new CappedAttestationQuotaSegmentLengthRule(3600),
                new CappedAttestationQuotaSalaryBandLengthRule(1000),
                new CappedAttestationQuotaCodeLengthRule(255),
                new CappedAttestationQuotaThresholdRangeRule(1.1, 4201.3),
            });
    }
}
