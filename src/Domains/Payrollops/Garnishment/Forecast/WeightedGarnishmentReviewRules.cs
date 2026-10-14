namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>A rule a weighted garnishment review must pass. Check returns its problems, or none.</summary>
public interface IWeightedGarnishmentReviewRule
{
    List<string> Check(WeightedGarnishmentReview item);
}

public sealed record WeightedGarnishmentReviewCodeLengthRule(int MaxLength) : IWeightedGarnishmentReviewRule
{
    public List<string> Check(WeightedGarnishmentReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedGarnishmentReviewPinnedGuardRule(double Limit) : IWeightedGarnishmentReviewRule
{
    public List<string> Check(WeightedGarnishmentReview item)
    {
        if (item.Pinned && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record WeightedGarnishmentReviewBudgetRangeRule(double Min, double Max) : IWeightedGarnishmentReviewRule
{
    public List<string> Check(WeightedGarnishmentReview item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedGarnishmentReviewRequiredCodeRule(string Message) : IWeightedGarnishmentReviewRule
{
    public List<string> Check(WeightedGarnishmentReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedGarnishmentReviewRequiredTitleRule(string Message) : IWeightedGarnishmentReviewRule
{
    public List<string> Check(WeightedGarnishmentReview item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedGarnishmentReviewRequiredTimezoneRule(string Message) : IWeightedGarnishmentReviewRule
{
    public List<string> Check(WeightedGarnishmentReview item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted garnishment review.</summary>
public sealed record WeightedGarnishmentReviewValidator(List<IWeightedGarnishmentReviewRule> Rules)
{
    public List<string> Validate(WeightedGarnishmentReview item)
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

    public void RequireValid(WeightedGarnishmentReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedGarnishmentReview> items)
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

/// <summary>Functions of weighted garnishment review rules.</summary>
public static class WeightedGarnishmentReviewRules
{
    public static WeightedGarnishmentReviewValidator DefaultWeightedGarnishmentReviewValidator()
    {
        return new WeightedGarnishmentReviewValidator(
            new List<IWeightedGarnishmentReviewRule>
            {
                new WeightedGarnishmentReviewCodeLengthRule(255),
                new WeightedGarnishmentReviewPinnedGuardRule(0.1),
                new WeightedGarnishmentReviewBudgetRangeRule(6.1, 960.0),
                new WeightedGarnishmentReviewRequiredCodeRule("weighted garnishment review needs this field"),
                new WeightedGarnishmentReviewRequiredTitleRule("weighted garnishment review needs this field"),
                new WeightedGarnishmentReviewRequiredTimezoneRule("weighted garnishment review needs an entry"),
            });
    }
}
