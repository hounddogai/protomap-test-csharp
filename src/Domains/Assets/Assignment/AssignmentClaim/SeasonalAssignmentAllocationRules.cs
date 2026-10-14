namespace Acme.Workforce.Assets.Assignment.AssignmentClaim;

/// <summary>A rule a seasonal assignment allocation must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAssignmentAllocationRule
{
    List<string> Check(SeasonalAssignmentAllocation item);
}

public sealed record SeasonalAssignmentAllocationMarginRangeRule(double Min, double Max) : ISeasonalAssignmentAllocationRule
{
    public List<string> Check(SeasonalAssignmentAllocation item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAssignmentAllocationRequiredCodeRule(string Message) : ISeasonalAssignmentAllocationRule
{
    public List<string> Check(SeasonalAssignmentAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAssignmentAllocationCodeLengthRule(int MaxLength) : ISeasonalAssignmentAllocationRule
{
    public List<string> Check(SeasonalAssignmentAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal assignment allocation.</summary>
public sealed record SeasonalAssignmentAllocationValidator(List<ISeasonalAssignmentAllocationRule> Rules)
{
    public List<string> Validate(SeasonalAssignmentAllocation item)
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

    public void RequireValid(SeasonalAssignmentAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAssignmentAllocation> items)
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

/// <summary>Functions of seasonal assignment allocation rules.</summary>
public static class SeasonalAssignmentAllocationRules
{
    public static SeasonalAssignmentAllocationValidator DefaultSeasonalAssignmentAllocationValidator()
    {
        return new SeasonalAssignmentAllocationValidator(
            new List<ISeasonalAssignmentAllocationRule>
            {
                new SeasonalAssignmentAllocationMarginRangeRule(7.0, 3922.0),
                new SeasonalAssignmentAllocationRequiredCodeRule("seasonal assignment allocation needs a value"),
                new SeasonalAssignmentAllocationCodeLengthRule(16),
            });
    }
}
