namespace Acme.Workforce.Compliance.Attestation.Segment;

/// <summary>A rule a attestation ledger must pass. Check returns its problems, or none.</summary>
public interface IAttestationLedgerRule
{
    List<string> Check(AttestationLedger item);
}

public sealed record AttestationLedgerRegionLengthRule(int MaxLength) : IAttestationLedgerRule
{
    public List<string> Check(AttestationLedger item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationLedgerRequiredLabelRule(string Message) : IAttestationLedgerRule
{
    public List<string> Check(AttestationLedger item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationLedgerCodeLengthRule(int MaxLength) : IAttestationLedgerRule
{
    public List<string> Check(AttestationLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationLedgerQuantityRangeRule(int Min, int Max) : IAttestationLedgerRule
{
    public List<string> Check(AttestationLedger item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a attestation ledger.</summary>
public sealed record AttestationLedgerValidator(List<IAttestationLedgerRule> Rules)
{
    public List<string> Validate(AttestationLedger item)
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

    public void RequireValid(AttestationLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationLedger> items)
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

/// <summary>Functions of attestation ledger rules.</summary>
public static class AttestationLedgerRules
{
    public static AttestationLedgerValidator DefaultAttestationLedgerValidator()
    {
        return new AttestationLedgerValidator(
            new List<IAttestationLedgerRule>
            {
                new AttestationLedgerRegionLengthRule(32),
                new AttestationLedgerRequiredLabelRule("attestation ledger needs an entry"),
                new AttestationLedgerCodeLengthRule(86400),
                new AttestationLedgerQuantityRangeRule(64, 86400),
            });
    }
}
