namespace Acme.Workforce.Compliance.Attestation.Quota;

/// <summary>A rule a attestation mapping must pass. Check returns its problems, or none.</summary>
public interface IAttestationMappingRule
{
    List<string> Check(AttestationMapping item);
}

public sealed record AttestationMappingRequiredCodeRule(string Message) : IAttestationMappingRule
{
    public List<string> Check(AttestationMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationMappingWeightRangeRule(double Min, double Max) : IAttestationMappingRule
{
    public List<string> Check(AttestationMapping item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationMappingCodeLengthRule(int MaxLength) : IAttestationMappingRule
{
    public List<string> Check(AttestationMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation mapping.</summary>
public sealed record AttestationMappingValidator(List<IAttestationMappingRule> Rules)
{
    public List<string> Validate(AttestationMapping item)
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

    public void RequireValid(AttestationMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationMapping> items)
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

/// <summary>Functions of attestation mapping rules.</summary>
public static class AttestationMappingRules
{
    public static AttestationMappingValidator DefaultAttestationMappingValidator()
    {
        return new AttestationMappingValidator(
            new List<IAttestationMappingRule>
            {
                new AttestationMappingRequiredCodeRule("attestation mapping needs a value"),
                new AttestationMappingWeightRangeRule(2.4, 3035.7),
                new AttestationMappingCodeLengthRule(80),
            });
    }
}
