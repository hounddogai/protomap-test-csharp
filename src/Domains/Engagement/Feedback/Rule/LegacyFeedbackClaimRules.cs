namespace Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>A rule a legacy feedback claim must pass. Check returns its problems, or none.</summary>
public interface ILegacyFeedbackClaimRule
{
    List<string> Check(LegacyFeedbackClaim item);
}

public sealed record LegacyFeedbackClaimRequiredCodeRule(string Message) : ILegacyFeedbackClaimRule
{
    public List<string> Check(LegacyFeedbackClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyFeedbackClaimRequiredLocationRule(string Message) : ILegacyFeedbackClaimRule
{
    public List<string> Check(LegacyFeedbackClaim item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyFeedbackClaimCodeLengthRule(int MaxLength) : ILegacyFeedbackClaimRule
{
    public List<string> Check(LegacyFeedbackClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyFeedbackClaimCostRangeRule(double Min, double Max) : ILegacyFeedbackClaimRule
{
    public List<string> Check(LegacyFeedbackClaim item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyFeedbackClaimRequiredCountryRule(string Message) : ILegacyFeedbackClaimRule
{
    public List<string> Check(LegacyFeedbackClaim item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy feedback claim.</summary>
public sealed record LegacyFeedbackClaimValidator(List<ILegacyFeedbackClaimRule> Rules)
{
    public List<string> Validate(LegacyFeedbackClaim item)
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

    public void RequireValid(LegacyFeedbackClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyFeedbackClaim> items)
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

/// <summary>Functions of legacy feedback claim rules.</summary>
public static class LegacyFeedbackClaimRules
{
    public static LegacyFeedbackClaimValidator DefaultLegacyFeedbackClaimValidator()
    {
        return new LegacyFeedbackClaimValidator(
            new List<ILegacyFeedbackClaimRule>
            {
                new LegacyFeedbackClaimRequiredCodeRule("legacy feedback claim needs an entry"),
                new LegacyFeedbackClaimRequiredLocationRule("legacy feedback claim needs a value"),
                new LegacyFeedbackClaimCodeLengthRule(255),
                new LegacyFeedbackClaimCostRangeRule(7.4, 1047.0),
                new LegacyFeedbackClaimRequiredCountryRule("legacy feedback claim needs an entry"),
            });
    }
}
