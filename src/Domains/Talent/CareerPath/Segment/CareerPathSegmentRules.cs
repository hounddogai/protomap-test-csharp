namespace Acme.Workforce.Talent.CareerPath.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a career path segment must pass. Check returns its problems, or none.</summary>
public interface ICareerPathSegmentRule
{
    List<string> Check(CareerPathSegment item);
}

public sealed record CareerPathSegmentCodePrefixRule(string Prefix) : ICareerPathSegmentRule
{
    public List<string> Check(CareerPathSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathSegmentRevisionRangeRule(int Min, int Max) : ICareerPathSegmentRule
{
    public List<string> Check(CareerPathSegment item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathSegmentCostCenterLengthRule(int MaxLength) : ICareerPathSegmentRule
{
    public List<string> Check(CareerPathSegment item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathSegmentCeilingRangeRule(double Min, double Max) : ICareerPathSegmentRule
{
    public List<string> Check(CareerPathSegment item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CareerPathSegmentAllowedGradeRule(List<string> Allowed) : ICareerPathSegmentRule
{
    public List<string> Check(CareerPathSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path segment.</summary>
public sealed record CareerPathSegmentValidator(List<ICareerPathSegmentRule> Rules)
{
    public List<string> Validate(CareerPathSegment item)
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

    public void RequireValid(CareerPathSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathSegment> items)
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

/// <summary>Functions of career path segment rules.</summary>
public static class CareerPathSegmentRules
{
    public static CareerPathSegmentValidator DefaultCareerPathSegmentValidator()
    {
        return new CareerPathSegmentValidator(
            new List<ICareerPathSegmentRule>
            {
                new CareerPathSegmentCodePrefixRule("ACM"),
                new CareerPathSegmentRevisionRangeRule(3600, 3600),
                new CareerPathSegmentCostCenterLengthRule(120),
                new CareerPathSegmentCeilingRangeRule(1.8, 3687.7),
                new CareerPathSegmentAllowedGradeRule(new List<string> { "global", "pending", "lenient" }),
            });
    }
}
