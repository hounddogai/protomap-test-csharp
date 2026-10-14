namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

/// <summary>A rule a attestation group must pass. Check returns its problems, or none.</summary>
public interface IAttestationGroupRule
{
    List<string> Check(AttestationGroup item);
}

public sealed record AttestationGroupPhoneLengthRule(int MaxLength) : IAttestationGroupRule
{
    public List<string> Check(AttestationGroup item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationGroupFloorValueRangeRule(double Min, double Max) : IAttestationGroupRule
{
    public List<string> Check(AttestationGroup item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationGroupRequiredPhoneRule(string Message) : IAttestationGroupRule
{
    public List<string> Check(AttestationGroup item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationGroupRequiredCodeRule(string Message) : IAttestationGroupRule
{
    public List<string> Check(AttestationGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation group.</summary>
public sealed record AttestationGroupValidator(List<IAttestationGroupRule> Rules)
{
    public List<string> Validate(AttestationGroup item)
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

    public void RequireValid(AttestationGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationGroup> items)
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

/// <summary>Functions of attestation group rules.</summary>
public static class AttestationGroupRules
{
    public static AttestationGroupValidator DefaultAttestationGroupValidator()
    {
        return new AttestationGroupValidator(
            new List<IAttestationGroupRule>
            {
                new AttestationGroupPhoneLengthRule(255),
                new AttestationGroupFloorValueRangeRule(0.8, 4735.7),
                new AttestationGroupRequiredPhoneRule("attestation group needs a value"),
                new AttestationGroupRequiredCodeRule("attestation group needs this field"),
            });
    }
}
