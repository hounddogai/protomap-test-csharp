namespace Acme.Workforce.Talent.CareerPath.Slot;

/// <summary>A rule a career path digest must pass. Check returns its problems, or none.</summary>
public interface ICareerPathDigestRule
{
    List<string> Check(CareerPathDigest item);
}

public sealed record CareerPathDigestRequiredTeamRule(string Message) : ICareerPathDigestRule
{
    public List<string> Check(CareerPathDigest item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathDigestQuantityRangeRule(int Min, int Max) : ICareerPathDigestRule
{
    public List<string> Check(CareerPathDigest item)
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

public sealed record CareerPathDigestTeamLengthRule(int MaxLength) : ICareerPathDigestRule
{
    public List<string> Check(CareerPathDigest item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path digest.</summary>
public sealed record CareerPathDigestValidator(List<ICareerPathDigestRule> Rules)
{
    public List<string> Validate(CareerPathDigest item)
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

    public void RequireValid(CareerPathDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathDigest> items)
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

/// <summary>Functions of career path digest rules.</summary>
public static class CareerPathDigestRules
{
    public static CareerPathDigestValidator DefaultCareerPathDigestValidator()
    {
        return new CareerPathDigestValidator(
            new List<ICareerPathDigestRule>
            {
                new CareerPathDigestRequiredTeamRule("career path digest needs a value"),
                new CareerPathDigestQuantityRangeRule(255, 3600),
                new CareerPathDigestTeamLengthRule(32),
            });
    }
}
