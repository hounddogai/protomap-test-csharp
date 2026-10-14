namespace Acme.Workforce.Absence.Accrual.Manifest;

/// <summary>A rule a accrual agreement must pass. Check returns its problems, or none.</summary>
public interface IAccrualAgreementRule
{
    List<string> Check(AccrualAgreement item);
}

public sealed record AccrualAgreementRevisionRangeRule(int Min, int Max) : IAccrualAgreementRule
{
    public List<string> Check(AccrualAgreement item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualAgreementCodePrefixRule(string Prefix) : IAccrualAgreementRule
{
    public List<string> Check(AccrualAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualAgreementEndsAtRangeRule(int Min, int Max) : IAccrualAgreementRule
{
    public List<string> Check(AccrualAgreement item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a accrual agreement.</summary>
public sealed record AccrualAgreementValidator(List<IAccrualAgreementRule> Rules)
{
    public List<string> Validate(AccrualAgreement item)
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

    public void RequireValid(AccrualAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualAgreement> items)
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

/// <summary>Functions of accrual agreement rules.</summary>
public static class AccrualAgreementRules
{
    public static AccrualAgreementValidator DefaultAccrualAgreementValidator()
    {
        return new AccrualAgreementValidator(
            new List<IAccrualAgreementRule>
            {
                new AccrualAgreementRevisionRangeRule(16, 64),
                new AccrualAgreementCodePrefixRule("X"),
                new AccrualAgreementEndsAtRangeRule(80, 120),
            });
    }
}
