namespace Acme.Workforce.Engagement.PulseSurvey.Claim;

/// <summary>A rule a pending pulse survey claim must pass. Check returns its problems, or none.</summary>
public interface IPendingPulseSurveyClaimRule
{
    List<string> Check(PendingPulseSurveyClaim item);
}

public sealed record PendingPulseSurveyClaimRequiredCodeRule(string Message) : IPendingPulseSurveyClaimRule
{
    public List<string> Check(PendingPulseSurveyClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingPulseSurveyClaimDepartmentLengthRule(int MaxLength) : IPendingPulseSurveyClaimRule
{
    public List<string> Check(PendingPulseSurveyClaim item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPulseSurveyClaimNameLengthRule(int MaxLength) : IPendingPulseSurveyClaimRule
{
    public List<string> Check(PendingPulseSurveyClaim item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPulseSurveyClaimRequiredNameRule(string Message) : IPendingPulseSurveyClaimRule
{
    public List<string> Check(PendingPulseSurveyClaim item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingPulseSurveyClaimRequiredDepartmentRule(string Message) : IPendingPulseSurveyClaimRule
{
    public List<string> Check(PendingPulseSurveyClaim item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending pulse survey claim.</summary>
public sealed record PendingPulseSurveyClaimValidator(List<IPendingPulseSurveyClaimRule> Rules)
{
    public List<string> Validate(PendingPulseSurveyClaim item)
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

    public void RequireValid(PendingPulseSurveyClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingPulseSurveyClaim> items)
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

/// <summary>Functions of pending pulse survey claim rules.</summary>
public static class PendingPulseSurveyClaimRules
{
    public static PendingPulseSurveyClaimValidator DefaultPendingPulseSurveyClaimValidator()
    {
        return new PendingPulseSurveyClaimValidator(
            new List<IPendingPulseSurveyClaimRule>
            {
                new PendingPulseSurveyClaimRequiredCodeRule("pending pulse survey claim needs an entry"),
                new PendingPulseSurveyClaimDepartmentLengthRule(255),
                new PendingPulseSurveyClaimNameLengthRule(255),
                new PendingPulseSurveyClaimRequiredNameRule("pending pulse survey claim needs this field"),
                new PendingPulseSurveyClaimRequiredDepartmentRule("pending pulse survey claim needs a value"),
            });
    }
}
