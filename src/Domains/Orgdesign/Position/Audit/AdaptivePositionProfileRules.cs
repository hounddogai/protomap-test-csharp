namespace Acme.Workforce.Orgdesign.Position.Audit;

/// <summary>A rule a adaptive position profile must pass. Check returns its problems, or none.</summary>
public interface IAdaptivePositionProfileRule
{
    List<string> Check(AdaptivePositionProfile item);
}

public sealed record AdaptivePositionProfileRequiredTitleRule(string Message) : IAdaptivePositionProfileRule
{
    public List<string> Check(AdaptivePositionProfile item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePositionProfileRequiredCodeRule(string Message) : IAdaptivePositionProfileRule
{
    public List<string> Check(AdaptivePositionProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePositionProfileSummaryLengthRule(int MaxLength) : IAdaptivePositionProfileRule
{
    public List<string> Check(AdaptivePositionProfile item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePositionProfileActiveGuardRule(double Limit) : IAdaptivePositionProfileRule
{
    public List<string> Check(AdaptivePositionProfile item)
    {
        if (item.Active && item.Threshold > Limit)
        {
            return new List<string> { $"threshold {item.Threshold} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePositionProfileRequiredDescriptionRule(string Message) : IAdaptivePositionProfileRule
{
    public List<string> Check(AdaptivePositionProfile item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePositionProfileRequiredSummaryRule(string Message) : IAdaptivePositionProfileRule
{
    public List<string> Check(AdaptivePositionProfile item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive position profile.</summary>
public sealed record AdaptivePositionProfileValidator(List<IAdaptivePositionProfileRule> Rules)
{
    public List<string> Validate(AdaptivePositionProfile item)
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

    public void RequireValid(AdaptivePositionProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptivePositionProfile> items)
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

/// <summary>Functions of adaptive position profile rules.</summary>
public static class AdaptivePositionProfileRules
{
    public static AdaptivePositionProfileValidator DefaultAdaptivePositionProfileValidator()
    {
        return new AdaptivePositionProfileValidator(
            new List<IAdaptivePositionProfileRule>
            {
                new AdaptivePositionProfileRequiredTitleRule("adaptive position profile needs a value"),
                new AdaptivePositionProfileRequiredCodeRule("adaptive position profile needs a value"),
                new AdaptivePositionProfileSummaryLengthRule(255),
                new AdaptivePositionProfileActiveGuardRule(7.3),
                new AdaptivePositionProfileRequiredDescriptionRule("adaptive position profile needs a value"),
                new AdaptivePositionProfileRequiredSummaryRule("adaptive position profile needs an entry"),
            });
    }
}
