namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>A rule a lenient scenario agreement must pass. Check returns its problems, or none.</summary>
public interface ILenientScenarioAgreementRule
{
    List<string> Check(LenientScenarioAgreement item);
}

public sealed record LenientScenarioAgreementRequiredCodeRule(string Message) : ILenientScenarioAgreementRule
{
    public List<string> Check(LenientScenarioAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientScenarioAgreementSummaryLengthRule(int MaxLength) : ILenientScenarioAgreementRule
{
    public List<string> Check(LenientScenarioAgreement item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientScenarioAgreementLabelLengthRule(int MaxLength) : ILenientScenarioAgreementRule
{
    public List<string> Check(LenientScenarioAgreement item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientScenarioAgreementVisibleGuardRule(double Limit) : ILenientScenarioAgreementRule
{
    public List<string> Check(LenientScenarioAgreement item)
    {
        if (item.Visible && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient scenario agreement.</summary>
public sealed record LenientScenarioAgreementValidator(List<ILenientScenarioAgreementRule> Rules)
{
    public List<string> Validate(LenientScenarioAgreement item)
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

    public void RequireValid(LenientScenarioAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientScenarioAgreement> items)
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

/// <summary>Functions of lenient scenario agreement rules.</summary>
public static class LenientScenarioAgreementRules
{
    public static LenientScenarioAgreementValidator DefaultLenientScenarioAgreementValidator()
    {
        return new LenientScenarioAgreementValidator(
            new List<ILenientScenarioAgreementRule>
            {
                new LenientScenarioAgreementRequiredCodeRule("lenient scenario agreement needs an entry"),
                new LenientScenarioAgreementSummaryLengthRule(1000),
                new LenientScenarioAgreementLabelLengthRule(86400),
                new LenientScenarioAgreementVisibleGuardRule(2.0),
            });
    }
}
