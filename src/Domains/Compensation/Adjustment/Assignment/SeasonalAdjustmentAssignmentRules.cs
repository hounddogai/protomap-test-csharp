namespace Acme.Workforce.Compensation.Adjustment.Assignment;

/// <summary>A rule a seasonal adjustment assignment must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAdjustmentAssignmentRule
{
    List<string> Check(SeasonalAdjustmentAssignment item);
}

public sealed record SeasonalAdjustmentAssignmentTitleLengthRule(int MaxLength) : ISeasonalAdjustmentAssignmentRule
{
    public List<string> Check(SeasonalAdjustmentAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAdjustmentAssignmentCodePrefixRule(string Prefix) : ISeasonalAdjustmentAssignmentRule
{
    public List<string> Check(SeasonalAdjustmentAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAdjustmentAssignmentRequiredCodeRule(string Message) : ISeasonalAdjustmentAssignmentRule
{
    public List<string> Check(SeasonalAdjustmentAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal adjustment assignment.</summary>
public sealed record SeasonalAdjustmentAssignmentValidator(List<ISeasonalAdjustmentAssignmentRule> Rules)
{
    public List<string> Validate(SeasonalAdjustmentAssignment item)
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

    public void RequireValid(SeasonalAdjustmentAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAdjustmentAssignment> items)
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

/// <summary>Functions of seasonal adjustment assignment rules.</summary>
public static class SeasonalAdjustmentAssignmentRules
{
    public static SeasonalAdjustmentAssignmentValidator DefaultSeasonalAdjustmentAssignmentValidator()
    {
        return new SeasonalAdjustmentAssignmentValidator(
            new List<ISeasonalAdjustmentAssignmentRule>
            {
                new SeasonalAdjustmentAssignmentTitleLengthRule(32),
                new SeasonalAdjustmentAssignmentCodePrefixRule("Q"),
                new SeasonalAdjustmentAssignmentRequiredCodeRule("seasonal adjustment assignment needs a value"),
            });
    }
}
