namespace Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>A rule a primary attestation policy must pass. Check returns its problems, or none.</summary>
public interface IPrimaryAttestationPolicyRule
{
    List<string> Check(PrimaryAttestationPolicy item);
}

public sealed record PrimaryAttestationPolicyDiscountRangeRule(double Min, double Max) : IPrimaryAttestationPolicyRule
{
    public List<string> Check(PrimaryAttestationPolicy item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAttestationPolicyCodeLengthRule(int MaxLength) : IPrimaryAttestationPolicyRule
{
    public List<string> Check(PrimaryAttestationPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAttestationPolicyNameLengthRule(int MaxLength) : IPrimaryAttestationPolicyRule
{
    public List<string> Check(PrimaryAttestationPolicy item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAttestationPolicyRequiredCodeRule(string Message) : IPrimaryAttestationPolicyRule
{
    public List<string> Check(PrimaryAttestationPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary attestation policy.</summary>
public sealed record PrimaryAttestationPolicyValidator(List<IPrimaryAttestationPolicyRule> Rules)
{
    public List<string> Validate(PrimaryAttestationPolicy item)
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

    public void RequireValid(PrimaryAttestationPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryAttestationPolicy> items)
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

/// <summary>Functions of primary attestation policy rules.</summary>
public static class PrimaryAttestationPolicyRules
{
    public static PrimaryAttestationPolicyValidator DefaultPrimaryAttestationPolicyValidator()
    {
        return new PrimaryAttestationPolicyValidator(
            new List<IPrimaryAttestationPolicyRule>
            {
                new PrimaryAttestationPolicyDiscountRangeRule(9.2, 1448.8),
                new PrimaryAttestationPolicyCodeLengthRule(80),
                new PrimaryAttestationPolicyNameLengthRule(86400),
                new PrimaryAttestationPolicyRequiredCodeRule("primary attestation policy needs a value"),
            });
    }
}
