namespace Acme.Workforce.Compliance.Attestation.Window;

/// <summary>A rule a attestation window must pass. Check returns its problems, or none.</summary>
public interface IAttestationWindowRule
{
    List<string> Check(AttestationWindow item);
}

public sealed record AttestationWindowTitleLengthRule(int MaxLength) : IAttestationWindowRule
{
    public List<string> Check(AttestationWindow item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationWindowWindowDaysRangeRule(int Min, int Max) : IAttestationWindowRule
{
    public List<string> Check(AttestationWindow item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationWindowCostCenterLengthRule(int MaxLength) : IAttestationWindowRule
{
    public List<string> Check(AttestationWindow item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation window.</summary>
public sealed record AttestationWindowValidator(List<IAttestationWindowRule> Rules)
{
    public List<string> Validate(AttestationWindow item)
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

    public void RequireValid(AttestationWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationWindow> items)
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

/// <summary>Functions of attestation window rules.</summary>
public static class AttestationWindowRules
{
    public static AttestationWindowValidator DefaultAttestationWindowValidator()
    {
        return new AttestationWindowValidator(
            new List<IAttestationWindowRule>
            {
                new AttestationWindowTitleLengthRule(16),
                new AttestationWindowWindowDaysRangeRule(32, 255),
                new AttestationWindowCostCenterLengthRule(16),
            });
    }
}
