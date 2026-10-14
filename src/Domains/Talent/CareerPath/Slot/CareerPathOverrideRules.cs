namespace Acme.Workforce.Talent.CareerPath.Slot;

/// <summary>A rule a career path override must pass. Check returns its problems, or none.</summary>
public interface ICareerPathOverrideRule
{
    List<string> Check(CareerPathOverride item);
}

public sealed record CareerPathOverrideRequiredChannelRule(string Message) : ICareerPathOverrideRule
{
    public List<string> Check(CareerPathOverride item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathOverrideChannelLengthRule(int MaxLength) : ICareerPathOverrideRule
{
    public List<string> Check(CareerPathOverride item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathOverrideCreatedAtRangeRule(int Min, int Max) : ICareerPathOverrideRule
{
    public List<string> Check(CareerPathOverride item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path override.</summary>
public sealed record CareerPathOverrideValidator(List<ICareerPathOverrideRule> Rules)
{
    public List<string> Validate(CareerPathOverride item)
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

    public void RequireValid(CareerPathOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathOverride> items)
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

/// <summary>Functions of career path override rules.</summary>
public static class CareerPathOverrideRules
{
    public static CareerPathOverrideValidator DefaultCareerPathOverrideValidator()
    {
        return new CareerPathOverrideValidator(
            new List<ICareerPathOverrideRule>
            {
                new CareerPathOverrideRequiredChannelRule("career path override needs an entry"),
                new CareerPathOverrideChannelLengthRule(255),
                new CareerPathOverrideCreatedAtRangeRule(255, 255),
            });
    }
}
