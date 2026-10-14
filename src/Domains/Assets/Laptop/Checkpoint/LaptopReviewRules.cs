namespace Acme.Workforce.Assets.Laptop.Checkpoint;

/// <summary>A rule a laptop review must pass. Check returns its problems, or none.</summary>
public interface ILaptopReviewRule
{
    List<string> Check(LaptopReview item);
}

public sealed record LaptopReviewPriorityRangeRule(int Min, int Max) : ILaptopReviewRule
{
    public List<string> Check(LaptopReview item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LaptopReviewJobCodeLengthRule(int MaxLength) : ILaptopReviewRule
{
    public List<string> Check(LaptopReview item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopReviewTitleLengthRule(int MaxLength) : ILaptopReviewRule
{
    public List<string> Check(LaptopReview item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopReviewVisibleGuardRule(int Limit) : ILaptopReviewRule
{
    public List<string> Check(LaptopReview item)
    {
        if (item.Visible && item.DurationMinutes > Limit)
        {
            return new List<string> { $"duration minutes {item.DurationMinutes} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop review.</summary>
public sealed record LaptopReviewValidator(List<ILaptopReviewRule> Rules)
{
    public List<string> Validate(LaptopReview item)
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

    public void RequireValid(LaptopReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopReview> items)
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

/// <summary>Functions of laptop review rules.</summary>
public static class LaptopReviewRules
{
    public static LaptopReviewValidator DefaultLaptopReviewValidator()
    {
        return new LaptopReviewValidator(
            new List<ILaptopReviewRule>
            {
                new LaptopReviewPriorityRangeRule(32, 3600),
                new LaptopReviewJobCodeLengthRule(16),
                new LaptopReviewTitleLengthRule(120),
                new LaptopReviewVisibleGuardRule(255),
            });
    }
}
