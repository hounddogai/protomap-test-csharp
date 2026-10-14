namespace Acme.Workforce.Workplace.Occupancy.Review;

/// <summary>A rule a occupancy review must pass. Check returns its problems, or none.</summary>
public interface IOccupancyReviewRule
{
    List<string> Check(OccupancyReview item);
}

public sealed record OccupancyReviewLabelLengthRule(int MaxLength) : IOccupancyReviewRule
{
    public List<string> Check(OccupancyReview item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyReviewRequiredCodeRule(string Message) : IOccupancyReviewRule
{
    public List<string> Check(OccupancyReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyReviewLeaveDaysRangeRule(int Min, int Max) : IOccupancyReviewRule
{
    public List<string> Check(OccupancyReview item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OccupancyReviewQuantityRangeRule(int Min, int Max) : IOccupancyReviewRule
{
    public List<string> Check(OccupancyReview item)
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

public sealed record OccupancyReviewCapacityRangeRule(int Min, int Max) : IOccupancyReviewRule
{
    public List<string> Check(OccupancyReview item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyReviewCodeLengthRule(int MaxLength) : IOccupancyReviewRule
{
    public List<string> Check(OccupancyReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy review.</summary>
public sealed record OccupancyReviewValidator(List<IOccupancyReviewRule> Rules)
{
    public List<string> Validate(OccupancyReview item)
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

    public void RequireValid(OccupancyReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyReview> items)
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

/// <summary>Functions of occupancy review rules.</summary>
public static class OccupancyReviewRules
{
    public static OccupancyReviewValidator DefaultOccupancyReviewValidator()
    {
        return new OccupancyReviewValidator(
            new List<IOccupancyReviewRule>
            {
                new OccupancyReviewLabelLengthRule(86400),
                new OccupancyReviewRequiredCodeRule("occupancy review needs a value"),
                new OccupancyReviewLeaveDaysRangeRule(64, 1000),
                new OccupancyReviewQuantityRangeRule(16, 32),
                new OccupancyReviewCapacityRangeRule(16, 80),
                new OccupancyReviewCodeLengthRule(120),
            });
    }
}
