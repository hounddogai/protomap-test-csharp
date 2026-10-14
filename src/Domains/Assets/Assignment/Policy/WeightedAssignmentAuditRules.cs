namespace Acme.Workforce.Assets.Assignment.Policy;

/// <summary>A rule a weighted assignment audit must pass. Check returns its problems, or none.</summary>
public interface IWeightedAssignmentAuditRule
{
    List<string> Check(WeightedAssignmentAudit item);
}

public sealed record WeightedAssignmentAuditRatingRangeRule(double Min, double Max) : IWeightedAssignmentAuditRule
{
    public List<string> Check(WeightedAssignmentAudit item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedAssignmentAuditCodePrefixRule(string Prefix) : IWeightedAssignmentAuditRule
{
    public List<string> Check(WeightedAssignmentAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAssignmentAuditRequiredTitleRule(string Message) : IWeightedAssignmentAuditRule
{
    public List<string> Check(WeightedAssignmentAudit item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted assignment audit.</summary>
public sealed record WeightedAssignmentAuditValidator(List<IWeightedAssignmentAuditRule> Rules)
{
    public List<string> Validate(WeightedAssignmentAudit item)
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

    public void RequireValid(WeightedAssignmentAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedAssignmentAudit> items)
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

/// <summary>Functions of weighted assignment audit rules.</summary>
public static class WeightedAssignmentAuditRules
{
    public static WeightedAssignmentAuditValidator DefaultWeightedAssignmentAuditValidator()
    {
        return new WeightedAssignmentAuditValidator(
            new List<IWeightedAssignmentAuditRule>
            {
                new WeightedAssignmentAuditRatingRangeRule(5.0, 3090.2),
                new WeightedAssignmentAuditCodePrefixRule("OPS"),
                new WeightedAssignmentAuditRequiredTitleRule("weighted assignment audit needs a value"),
            });
    }
}
