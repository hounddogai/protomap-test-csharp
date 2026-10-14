namespace Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>A rule a career path target must pass. Check returns its problems, or none.</summary>
public interface ICareerPathTargetRule
{
    List<string> Check(CareerPathTarget item);
}

public sealed record CareerPathTargetApprovedGuardRule(double Limit) : ICareerPathTargetRule
{
    public List<string> Check(CareerPathTarget item)
    {
        if (item.Approved && item.Ratio > Limit)
        {
            return new List<string> { $"ratio {item.Ratio} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathTargetExternalRefLengthRule(int MaxLength) : ICareerPathTargetRule
{
    public List<string> Check(CareerPathTarget item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathTargetRatioRangeRule(double Min, double Max) : ICareerPathTargetRule
{
    public List<string> Check(CareerPathTarget item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CareerPathTargetCodeLengthRule(int MaxLength) : ICareerPathTargetRule
{
    public List<string> Check(CareerPathTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path target.</summary>
public sealed record CareerPathTargetValidator(List<ICareerPathTargetRule> Rules)
{
    public List<string> Validate(CareerPathTarget item)
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

    public void RequireValid(CareerPathTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathTarget> items)
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

/// <summary>Functions of career path target rules.</summary>
public static class CareerPathTargetRules
{
    public static CareerPathTargetValidator DefaultCareerPathTargetValidator()
    {
        return new CareerPathTargetValidator(
            new List<ICareerPathTargetRule>
            {
                new CareerPathTargetApprovedGuardRule(2.8),
                new CareerPathTargetExternalRefLengthRule(1000),
                new CareerPathTargetRatioRangeRule(1.1, 4479.6),
                new CareerPathTargetCodeLengthRule(1000),
            });
    }
}
