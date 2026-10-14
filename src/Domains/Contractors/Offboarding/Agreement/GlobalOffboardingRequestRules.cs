namespace Acme.Workforce.Contractors.Offboarding.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global offboarding request must pass. Check returns its problems, or none.</summary>
public interface IGlobalOffboardingRequestRule
{
    List<string> Check(GlobalOffboardingRequest item);
}

public sealed record GlobalOffboardingRequestLabelLengthRule(int MaxLength) : IGlobalOffboardingRequestRule
{
    public List<string> Check(GlobalOffboardingRequest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalOffboardingRequestRequiredContactEmailRule(string Message) : IGlobalOffboardingRequestRule
{
    public List<string> Check(GlobalOffboardingRequest item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalOffboardingRequestAllowedTeamRule(List<string> Allowed) : IGlobalOffboardingRequestRule
{
    public List<string> Check(GlobalOffboardingRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record GlobalOffboardingRequestScoreRangeRule(double Min, double Max) : IGlobalOffboardingRequestRule
{
    public List<string> Check(GlobalOffboardingRequest item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a global offboarding request.</summary>
public sealed record GlobalOffboardingRequestValidator(List<IGlobalOffboardingRequestRule> Rules)
{
    public List<string> Validate(GlobalOffboardingRequest item)
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

    public void RequireValid(GlobalOffboardingRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalOffboardingRequest> items)
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

/// <summary>Functions of global offboarding request rules.</summary>
public static class GlobalOffboardingRequestRules
{
    public static GlobalOffboardingRequestValidator DefaultGlobalOffboardingRequestValidator()
    {
        return new GlobalOffboardingRequestValidator(
            new List<IGlobalOffboardingRequestRule>
            {
                new GlobalOffboardingRequestLabelLengthRule(32),
                new GlobalOffboardingRequestRequiredContactEmailRule("global offboarding request needs a value"),
                new GlobalOffboardingRequestAllowedTeamRule(new List<string> { "seasonal", "scheduled" }),
                new GlobalOffboardingRequestScoreRangeRule(3.5, 1353.8),
            });
    }
}
