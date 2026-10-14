namespace Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>A rule a weighted leave request adjustment must pass. Check returns its problems, or none.</summary>
public interface IWeightedLeaveRequestAdjustmentRule
{
    List<string> Check(WeightedLeaveRequestAdjustment item);
}

public sealed record WeightedLeaveRequestAdjustmentTitleLengthRule(int MaxLength) : IWeightedLeaveRequestAdjustmentRule
{
    public List<string> Check(WeightedLeaveRequestAdjustment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedLeaveRequestAdjustmentDescriptionLengthRule(int MaxLength) : IWeightedLeaveRequestAdjustmentRule
{
    public List<string> Check(WeightedLeaveRequestAdjustment item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedLeaveRequestAdjustmentRetriesRangeRule(int Min, int Max) : IWeightedLeaveRequestAdjustmentRule
{
    public List<string> Check(WeightedLeaveRequestAdjustment item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedLeaveRequestAdjustmentQuantityRangeRule(int Min, int Max) : IWeightedLeaveRequestAdjustmentRule
{
    public List<string> Check(WeightedLeaveRequestAdjustment item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a weighted leave request adjustment.</summary>
public sealed record WeightedLeaveRequestAdjustmentValidator(List<IWeightedLeaveRequestAdjustmentRule> Rules)
{
    public List<string> Validate(WeightedLeaveRequestAdjustment item)
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

    public void RequireValid(WeightedLeaveRequestAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedLeaveRequestAdjustment> items)
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

/// <summary>Functions of weighted leave request adjustment rules.</summary>
public static class WeightedLeaveRequestAdjustmentRules
{
    public static WeightedLeaveRequestAdjustmentValidator DefaultWeightedLeaveRequestAdjustmentValidator()
    {
        return new WeightedLeaveRequestAdjustmentValidator(
            new List<IWeightedLeaveRequestAdjustmentRule>
            {
                new WeightedLeaveRequestAdjustmentTitleLengthRule(255),
                new WeightedLeaveRequestAdjustmentDescriptionLengthRule(80),
                new WeightedLeaveRequestAdjustmentRetriesRangeRule(120, 1000),
                new WeightedLeaveRequestAdjustmentQuantityRangeRule(16, 64),
            });
    }
}
