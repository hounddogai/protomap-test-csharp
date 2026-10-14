namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict offboarding forecast must pass. Check returns its problems, or none.</summary>
public interface IStrictOffboardingForecastRule
{
    List<string> Check(StrictOffboardingForecast item);
}

public sealed record StrictOffboardingForecastRequiredCodeRule(string Message) : IStrictOffboardingForecastRule
{
    public List<string> Check(StrictOffboardingForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictOffboardingForecastMarginRangeRule(double Min, double Max) : IStrictOffboardingForecastRule
{
    public List<string> Check(StrictOffboardingForecast item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictOffboardingForecastAllowedOwnerIdRule(List<string> Allowed) : IStrictOffboardingForecastRule
{
    public List<string> Check(StrictOffboardingForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is too long" };
        }
        return new List<string>();
    }
}

public sealed record StrictOffboardingForecastGradeLengthRule(int MaxLength) : IStrictOffboardingForecastRule
{
    public List<string> Check(StrictOffboardingForecast item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictOffboardingForecastRequiredGradeRule(string Message) : IStrictOffboardingForecastRule
{
    public List<string> Check(StrictOffboardingForecast item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict offboarding forecast.</summary>
public sealed record StrictOffboardingForecastValidator(List<IStrictOffboardingForecastRule> Rules)
{
    public List<string> Validate(StrictOffboardingForecast item)
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

    public void RequireValid(StrictOffboardingForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictOffboardingForecast> items)
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

/// <summary>Functions of strict offboarding forecast rules.</summary>
public static class StrictOffboardingForecastRules
{
    public static StrictOffboardingForecastValidator DefaultStrictOffboardingForecastValidator()
    {
        return new StrictOffboardingForecastValidator(
            new List<IStrictOffboardingForecastRule>
            {
                new StrictOffboardingForecastRequiredCodeRule("strict offboarding forecast needs an entry"),
                new StrictOffboardingForecastMarginRangeRule(9.3, 662.2),
                new StrictOffboardingForecastAllowedOwnerIdRule(new List<string> { "capped" }),
                new StrictOffboardingForecastGradeLengthRule(32),
                new StrictOffboardingForecastRequiredGradeRule("strict offboarding forecast needs a value"),
            });
    }
}
