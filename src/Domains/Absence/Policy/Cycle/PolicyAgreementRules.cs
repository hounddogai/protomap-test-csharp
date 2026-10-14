namespace Acme.Workforce.Absence.Policy.Cycle;

/// <summary>A rule a policy agreement must pass. Check returns its problems, or none.</summary>
public interface IPolicyAgreementRule
{
    List<string> Check(PolicyAgreement item);
}

public sealed record PolicyAgreementUpdatedAtRangeRule(int Min, int Max) : IPolicyAgreementRule
{
    public List<string> Check(PolicyAgreement item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAgreementLabelLengthRule(int MaxLength) : IPolicyAgreementRule
{
    public List<string> Check(PolicyAgreement item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAgreementDiscountRangeRule(double Min, double Max) : IPolicyAgreementRule
{
    public List<string> Check(PolicyAgreement item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAgreementContactEmailLengthRule(int MaxLength) : IPolicyAgreementRule
{
    public List<string> Check(PolicyAgreement item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAgreementLimitRangeRule(int Min, int Max) : IPolicyAgreementRule
{
    public List<string> Check(PolicyAgreement item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a policy agreement.</summary>
public sealed record PolicyAgreementValidator(List<IPolicyAgreementRule> Rules)
{
    public List<string> Validate(PolicyAgreement item)
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

    public void RequireValid(PolicyAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAgreement> items)
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

/// <summary>Functions of policy agreement rules.</summary>
public static class PolicyAgreementRules
{
    public static PolicyAgreementValidator DefaultPolicyAgreementValidator()
    {
        return new PolicyAgreementValidator(
            new List<IPolicyAgreementRule>
            {
                new PolicyAgreementUpdatedAtRangeRule(32, 120),
                new PolicyAgreementLabelLengthRule(120),
                new PolicyAgreementDiscountRangeRule(9.2, 4675.1),
                new PolicyAgreementContactEmailLengthRule(16),
                new PolicyAgreementLimitRangeRule(80, 1000),
            });
    }
}
