namespace Acme.Workforce.Talent.CareerPath.Review;

/// <summary>A rule a career path review must pass. Check returns its problems, or none.</summary>
public interface ICareerPathReviewRule
{
    List<string> Check(CareerPathReview item);
}

public sealed record CareerPathReviewRequiredCodeRule(string Message) : ICareerPathReviewRule
{
    public List<string> Check(CareerPathReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathReviewWeightRangeRule(double Min, double Max) : ICareerPathReviewRule
{
    public List<string> Check(CareerPathReview item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CareerPathReviewRequiredContactEmailRule(string Message) : ICareerPathReviewRule
{
    public List<string> Check(CareerPathReview item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathReviewCodeLengthRule(int MaxLength) : ICareerPathReviewRule
{
    public List<string> Check(CareerPathReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathReviewLabelLengthRule(int MaxLength) : ICareerPathReviewRule
{
    public List<string> Check(CareerPathReview item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path review.</summary>
public sealed record CareerPathReviewValidator(List<ICareerPathReviewRule> Rules)
{
    public List<string> Validate(CareerPathReview item)
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

    public void RequireValid(CareerPathReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathReview> items)
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

/// <summary>Functions of career path review rules.</summary>
public static class CareerPathReviewRules
{
    public static CareerPathReviewValidator DefaultCareerPathReviewValidator()
    {
        return new CareerPathReviewValidator(
            new List<ICareerPathReviewRule>
            {
                new CareerPathReviewRequiredCodeRule("career path review needs an entry"),
                new CareerPathReviewWeightRangeRule(3.0, 3708.1),
                new CareerPathReviewRequiredContactEmailRule("career path review needs this field"),
                new CareerPathReviewCodeLengthRule(64),
                new CareerPathReviewLabelLengthRule(120),
            });
    }
}
