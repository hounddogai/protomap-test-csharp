namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

/// <summary>A rule a attestation request must pass. Check returns its problems, or none.</summary>
public interface IAttestationRequestRule
{
    List<string> Check(AttestationRequest item);
}

public sealed record AttestationRequestLabelLengthRule(int MaxLength) : IAttestationRequestRule
{
    public List<string> Check(AttestationRequest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationRequestExpiresAtRangeRule(int Min, int Max) : IAttestationRequestRule
{
    public List<string> Check(AttestationRequest item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationRequestBatchSizeRangeRule(int Min, int Max) : IAttestationRequestRule
{
    public List<string> Check(AttestationRequest item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a attestation request.</summary>
public sealed record AttestationRequestValidator(List<IAttestationRequestRule> Rules)
{
    public List<string> Validate(AttestationRequest item)
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

    public void RequireValid(AttestationRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationRequest> items)
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

/// <summary>Functions of attestation request rules.</summary>
public static class AttestationRequestRules
{
    public static AttestationRequestValidator DefaultAttestationRequestValidator()
    {
        return new AttestationRequestValidator(
            new List<IAttestationRequestRule>
            {
                new AttestationRequestLabelLengthRule(255),
                new AttestationRequestExpiresAtRangeRule(64, 1000),
                new AttestationRequestBatchSizeRangeRule(80, 1000),
            });
    }
}
