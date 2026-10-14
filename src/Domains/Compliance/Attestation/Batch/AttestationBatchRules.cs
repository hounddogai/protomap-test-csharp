namespace Acme.Workforce.Compliance.Attestation.Batch;

/// <summary>A rule a attestation batch must pass. Check returns its problems, or none.</summary>
public interface IAttestationBatchRule
{
    List<string> Check(AttestationBatch item);
}

public sealed record AttestationBatchTenureMonthsRangeRule(int Min, int Max) : IAttestationBatchRule
{
    public List<string> Check(AttestationBatch item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationBatchCodeLengthRule(int MaxLength) : IAttestationBatchRule
{
    public List<string> Check(AttestationBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationBatchNameLengthRule(int MaxLength) : IAttestationBatchRule
{
    public List<string> Check(AttestationBatch item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation batch.</summary>
public sealed record AttestationBatchValidator(List<IAttestationBatchRule> Rules)
{
    public List<string> Validate(AttestationBatch item)
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

    public void RequireValid(AttestationBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationBatch> items)
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

/// <summary>Functions of attestation batch rules.</summary>
public static class AttestationBatchRules
{
    public static AttestationBatchValidator DefaultAttestationBatchValidator()
    {
        return new AttestationBatchValidator(
            new List<IAttestationBatchRule>
            {
                new AttestationBatchTenureMonthsRangeRule(80, 120),
                new AttestationBatchCodeLengthRule(32),
                new AttestationBatchNameLengthRule(1000),
            });
    }
}
