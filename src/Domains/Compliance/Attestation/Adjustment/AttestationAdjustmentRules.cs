namespace Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>A rule a attestation adjustment must pass. Check returns its problems, or none.</summary>
public interface IAttestationAdjustmentRule
{
    List<string> Check(AttestationAdjustment item);
}

public sealed record AttestationAdjustmentTitleLengthRule(int MaxLength) : IAttestationAdjustmentRule
{
    public List<string> Check(AttestationAdjustment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationAdjustmentRequiredExternalRefRule(string Message) : IAttestationAdjustmentRule
{
    public List<string> Check(AttestationAdjustment item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationAdjustmentRatioRangeRule(double Min, double Max) : IAttestationAdjustmentRule
{
    public List<string> Check(AttestationAdjustment item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationAdjustmentCodeLengthRule(int MaxLength) : IAttestationAdjustmentRule
{
    public List<string> Check(AttestationAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation adjustment.</summary>
public sealed record AttestationAdjustmentValidator(List<IAttestationAdjustmentRule> Rules)
{
    public List<string> Validate(AttestationAdjustment item)
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

    public void RequireValid(AttestationAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationAdjustment> items)
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

/// <summary>Functions of attestation adjustment rules.</summary>
public static class AttestationAdjustmentRules
{
    public static AttestationAdjustmentValidator DefaultAttestationAdjustmentValidator()
    {
        return new AttestationAdjustmentValidator(
            new List<IAttestationAdjustmentRule>
            {
                new AttestationAdjustmentTitleLengthRule(16),
                new AttestationAdjustmentRequiredExternalRefRule("attestation adjustment needs an entry"),
                new AttestationAdjustmentRatioRangeRule(4.1, 2280.3),
                new AttestationAdjustmentCodeLengthRule(86400),
            });
    }
}
