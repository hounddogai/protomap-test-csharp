namespace Acme.Workforce.Talent.CareerPath.Variance;

/// <summary>A rule a career path variance must pass. Check returns its problems, or none.</summary>
public interface ICareerPathVarianceRule
{
    List<string> Check(CareerPathVariance item);
}

public sealed record CareerPathVarianceTitleLengthRule(int MaxLength) : ICareerPathVarianceRule
{
    public List<string> Check(CareerPathVariance item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathVarianceBudgetRangeRule(double Min, double Max) : ICareerPathVarianceRule
{
    public List<string> Check(CareerPathVariance item)
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

public sealed record CareerPathVarianceWindowDaysRangeRule(int Min, int Max) : ICareerPathVarianceRule
{
    public List<string> Check(CareerPathVariance item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CareerPathVarianceLocationLengthRule(int MaxLength) : ICareerPathVarianceRule
{
    public List<string> Check(CareerPathVariance item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathVarianceCodeLengthRule(int MaxLength) : ICareerPathVarianceRule
{
    public List<string> Check(CareerPathVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path variance.</summary>
public sealed record CareerPathVarianceValidator(List<ICareerPathVarianceRule> Rules)
{
    public List<string> Validate(CareerPathVariance item)
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

    public void RequireValid(CareerPathVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathVariance> items)
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

/// <summary>Functions of career path variance rules.</summary>
public static class CareerPathVarianceRules
{
    public static CareerPathVarianceValidator DefaultCareerPathVarianceValidator()
    {
        return new CareerPathVarianceValidator(
            new List<ICareerPathVarianceRule>
            {
                new CareerPathVarianceTitleLengthRule(255),
                new CareerPathVarianceBudgetRangeRule(2.9, 3132.7),
                new CareerPathVarianceWindowDaysRangeRule(16, 120),
                new CareerPathVarianceLocationLengthRule(120),
                new CareerPathVarianceCodeLengthRule(120),
            });
    }
}
