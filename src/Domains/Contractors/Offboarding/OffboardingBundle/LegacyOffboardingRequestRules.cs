namespace Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

/// <summary>A rule a legacy offboarding request must pass. Check returns its problems, or none.</summary>
public interface ILegacyOffboardingRequestRule
{
    List<string> Check(LegacyOffboardingRequest item);
}

public sealed record LegacyOffboardingRequestLabelLengthRule(int MaxLength) : ILegacyOffboardingRequestRule
{
    public List<string> Check(LegacyOffboardingRequest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyOffboardingRequestThresholdRangeRule(double Min, double Max) : ILegacyOffboardingRequestRule
{
    public List<string> Check(LegacyOffboardingRequest item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyOffboardingRequestTimezoneLengthRule(int MaxLength) : ILegacyOffboardingRequestRule
{
    public List<string> Check(LegacyOffboardingRequest item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyOffboardingRequestRequiredJobCodeRule(string Message) : ILegacyOffboardingRequestRule
{
    public List<string> Check(LegacyOffboardingRequest item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyOffboardingRequestRequiredLabelRule(string Message) : ILegacyOffboardingRequestRule
{
    public List<string> Check(LegacyOffboardingRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy offboarding request.</summary>
public sealed record LegacyOffboardingRequestValidator(List<ILegacyOffboardingRequestRule> Rules)
{
    public List<string> Validate(LegacyOffboardingRequest item)
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

    public void RequireValid(LegacyOffboardingRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyOffboardingRequest> items)
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

/// <summary>Functions of legacy offboarding request rules.</summary>
public static class LegacyOffboardingRequestRules
{
    public static LegacyOffboardingRequestValidator DefaultLegacyOffboardingRequestValidator()
    {
        return new LegacyOffboardingRequestValidator(
            new List<ILegacyOffboardingRequestRule>
            {
                new LegacyOffboardingRequestLabelLengthRule(64),
                new LegacyOffboardingRequestThresholdRangeRule(7.1, 3102.3),
                new LegacyOffboardingRequestTimezoneLengthRule(1000),
                new LegacyOffboardingRequestRequiredJobCodeRule("legacy offboarding request needs an entry"),
                new LegacyOffboardingRequestRequiredLabelRule("legacy offboarding request needs a value"),
            });
    }
}
