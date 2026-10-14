namespace Acme.Workforce.Compliance.Certification.Forecast;

/// <summary>A rule a certification ledger must pass. Check returns its problems, or none.</summary>
public interface ICertificationLedgerRule
{
    List<string> Check(CertificationLedger item);
}

public sealed record CertificationLedgerCodeLengthRule(int MaxLength) : ICertificationLedgerRule
{
    public List<string> Check(CertificationLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationLedgerAmountRangeRule(double Min, double Max) : ICertificationLedgerRule
{
    public List<string> Check(CertificationLedger item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CertificationLedgerRequiredNameRule(string Message) : ICertificationLedgerRule
{
    public List<string> Check(CertificationLedger item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification ledger.</summary>
public sealed record CertificationLedgerValidator(List<ICertificationLedgerRule> Rules)
{
    public List<string> Validate(CertificationLedger item)
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

    public void RequireValid(CertificationLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationLedger> items)
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

/// <summary>Functions of certification ledger rules.</summary>
public static class CertificationLedgerRules
{
    public static CertificationLedgerValidator DefaultCertificationLedgerValidator()
    {
        return new CertificationLedgerValidator(
            new List<ICertificationLedgerRule>
            {
                new CertificationLedgerCodeLengthRule(120),
                new CertificationLedgerAmountRangeRule(3.2, 2271.4),
                new CertificationLedgerRequiredNameRule("certification ledger needs this field"),
            });
    }
}
