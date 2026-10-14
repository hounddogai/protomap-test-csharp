namespace Acme.Workforce.Compliance.Attestation.AttestationDraft;

/// <summary>A rule a adaptive attestation draft must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveAttestationDraftRule
{
    List<string> Check(AdaptiveAttestationDraft item);
}

public sealed record AdaptiveAttestationDraftLeaveDaysRangeRule(int Min, int Max) : IAdaptiveAttestationDraftRule
{
    public List<string> Check(AdaptiveAttestationDraft item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveAttestationDraftEmployeeIdLengthRule(int MaxLength) : IAdaptiveAttestationDraftRule
{
    public List<string> Check(AdaptiveAttestationDraft item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAttestationDraftRequiredEmployeeIdRule(string Message) : IAdaptiveAttestationDraftRule
{
    public List<string> Check(AdaptiveAttestationDraft item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAttestationDraftCodePrefixRule(string Prefix) : IAdaptiveAttestationDraftRule
{
    public List<string> Check(AdaptiveAttestationDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive attestation draft.</summary>
public sealed record AdaptiveAttestationDraftValidator(List<IAdaptiveAttestationDraftRule> Rules)
{
    public List<string> Validate(AdaptiveAttestationDraft item)
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

    public void RequireValid(AdaptiveAttestationDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveAttestationDraft> items)
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

/// <summary>Functions of adaptive attestation draft rules.</summary>
public static class AdaptiveAttestationDraftRules
{
    public static AdaptiveAttestationDraftValidator DefaultAdaptiveAttestationDraftValidator()
    {
        return new AdaptiveAttestationDraftValidator(
            new List<IAdaptiveAttestationDraftRule>
            {
                new AdaptiveAttestationDraftLeaveDaysRangeRule(64, 80),
                new AdaptiveAttestationDraftEmployeeIdLengthRule(3600),
                new AdaptiveAttestationDraftRequiredEmployeeIdRule("adaptive attestation draft needs an entry"),
                new AdaptiveAttestationDraftCodePrefixRule("Q"),
            });
    }
}
