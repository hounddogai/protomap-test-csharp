namespace Acme.Workforce.Compliance.Attestation.Policy;

/// <summary>A rule a attestation policy must pass. Check returns its problems, or none.</summary>
public interface IAttestationPolicyRule
{
    List<string> Check(AttestationPolicy item);
}

public sealed record AttestationPolicySequenceRangeRule(int Min, int Max) : IAttestationPolicyRule
{
    public List<string> Check(AttestationPolicy item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationPolicyMarginRangeRule(double Min, double Max) : IAttestationPolicyRule
{
    public List<string> Check(AttestationPolicy item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationPolicyRequiredTitleRule(string Message) : IAttestationPolicyRule
{
    public List<string> Check(AttestationPolicy item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationPolicyDiscountRangeRule(double Min, double Max) : IAttestationPolicyRule
{
    public List<string> Check(AttestationPolicy item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationPolicyRequiredCodeRule(string Message) : IAttestationPolicyRule
{
    public List<string> Check(AttestationPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation policy.</summary>
public sealed record AttestationPolicyValidator(List<IAttestationPolicyRule> Rules)
{
    public List<string> Validate(AttestationPolicy item)
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

    public void RequireValid(AttestationPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationPolicy> items)
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

/// <summary>Functions of attestation policy rules.</summary>
public static class AttestationPolicyRules
{
    public static AttestationPolicyValidator DefaultAttestationPolicyValidator()
    {
        return new AttestationPolicyValidator(
            new List<IAttestationPolicyRule>
            {
                new AttestationPolicySequenceRangeRule(1000, 1000),
                new AttestationPolicyMarginRangeRule(6.5, 4599.9),
                new AttestationPolicyRequiredTitleRule("attestation policy needs a value"),
                new AttestationPolicyDiscountRangeRule(2.9, 2489.5),
                new AttestationPolicyRequiredCodeRule("attestation policy needs an entry"),
            });
    }
}
