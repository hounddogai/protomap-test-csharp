namespace Acme.Workforce.Compliance.Attestation.Draft;

/// <summary>A rule a attestation baseline must pass. Check returns its problems, or none.</summary>
public interface IAttestationBaselineRule
{
    List<string> Check(AttestationBaseline item);
}

public sealed record AttestationBaselineRequiredTitleRule(string Message) : IAttestationBaselineRule
{
    public List<string> Check(AttestationBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationBaselineNameLengthRule(int MaxLength) : IAttestationBaselineRule
{
    public List<string> Check(AttestationBaseline item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationBaselineRequiredNameRule(string Message) : IAttestationBaselineRule
{
    public List<string> Check(AttestationBaseline item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationBaselineOwnerIdLengthRule(int MaxLength) : IAttestationBaselineRule
{
    public List<string> Check(AttestationBaseline item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationBaselineTenureMonthsRangeRule(int Min, int Max) : IAttestationBaselineRule
{
    public List<string> Check(AttestationBaseline item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationBaselineRequiredOwnerIdRule(string Message) : IAttestationBaselineRule
{
    public List<string> Check(AttestationBaseline item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation baseline.</summary>
public sealed record AttestationBaselineValidator(List<IAttestationBaselineRule> Rules)
{
    public List<string> Validate(AttestationBaseline item)
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

    public void RequireValid(AttestationBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationBaseline> items)
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

/// <summary>Functions of attestation baseline rules.</summary>
public static class AttestationBaselineRules
{
    public static AttestationBaselineValidator DefaultAttestationBaselineValidator()
    {
        return new AttestationBaselineValidator(
            new List<IAttestationBaselineRule>
            {
                new AttestationBaselineRequiredTitleRule("attestation baseline needs an entry"),
                new AttestationBaselineNameLengthRule(120),
                new AttestationBaselineRequiredNameRule("attestation baseline needs a value"),
                new AttestationBaselineOwnerIdLengthRule(1000),
                new AttestationBaselineTenureMonthsRangeRule(255, 1000),
                new AttestationBaselineRequiredOwnerIdRule("attestation baseline needs a value"),
            });
    }
}
