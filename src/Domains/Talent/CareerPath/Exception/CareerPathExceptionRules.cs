namespace Acme.Workforce.Talent.CareerPath.Exception;

/// <summary>A rule a career path exception must pass. Check returns its problems, or none.</summary>
public interface ICareerPathExceptionRule
{
    List<string> Check(CareerPathException item);
}

public sealed record CareerPathExceptionCodeLengthRule(int MaxLength) : ICareerPathExceptionRule
{
    public List<string> Check(CareerPathException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathExceptionReviewerLengthRule(int MaxLength) : ICareerPathExceptionRule
{
    public List<string> Check(CareerPathException item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathExceptionStartsAtRangeRule(int Min, int Max) : ICareerPathExceptionRule
{
    public List<string> Check(CareerPathException item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CareerPathExceptionCodePrefixRule(string Prefix) : ICareerPathExceptionRule
{
    public List<string> Check(CareerPathException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathExceptionDurationMinutesRangeRule(int Min, int Max) : ICareerPathExceptionRule
{
    public List<string> Check(CareerPathException item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a career path exception.</summary>
public sealed record CareerPathExceptionValidator(List<ICareerPathExceptionRule> Rules)
{
    public List<string> Validate(CareerPathException item)
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

    public void RequireValid(CareerPathException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathException> items)
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

/// <summary>Functions of career path exception rules.</summary>
public static class CareerPathExceptionRules
{
    public static CareerPathExceptionValidator DefaultCareerPathExceptionValidator()
    {
        return new CareerPathExceptionValidator(
            new List<ICareerPathExceptionRule>
            {
                new CareerPathExceptionCodeLengthRule(32),
                new CareerPathExceptionReviewerLengthRule(255),
                new CareerPathExceptionStartsAtRangeRule(16, 80),
                new CareerPathExceptionCodePrefixRule("REF"),
                new CareerPathExceptionDurationMinutesRangeRule(255, 1000),
            });
    }
}
