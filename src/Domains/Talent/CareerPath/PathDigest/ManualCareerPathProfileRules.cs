namespace Acme.Workforce.Talent.CareerPath.PathDigest;

/// <summary>A rule a manual career path profile must pass. Check returns its problems, or none.</summary>
public interface IManualCareerPathProfileRule
{
    List<string> Check(ManualCareerPathProfile item);
}

public sealed record ManualCareerPathProfileRetriesRangeRule(int Min, int Max) : IManualCareerPathProfileRule
{
    public List<string> Check(ManualCareerPathProfile item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualCareerPathProfileNameLengthRule(int MaxLength) : IManualCareerPathProfileRule
{
    public List<string> Check(ManualCareerPathProfile item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualCareerPathProfileRequiredCategoryRule(string Message) : IManualCareerPathProfileRule
{
    public List<string> Check(ManualCareerPathProfile item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualCareerPathProfileChannelLengthRule(int MaxLength) : IManualCareerPathProfileRule
{
    public List<string> Check(ManualCareerPathProfile item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualCareerPathProfileCategoryLengthRule(int MaxLength) : IManualCareerPathProfileRule
{
    public List<string> Check(ManualCareerPathProfile item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual career path profile.</summary>
public sealed record ManualCareerPathProfileValidator(List<IManualCareerPathProfileRule> Rules)
{
    public List<string> Validate(ManualCareerPathProfile item)
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

    public void RequireValid(ManualCareerPathProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualCareerPathProfile> items)
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

/// <summary>Functions of manual career path profile rules.</summary>
public static class ManualCareerPathProfileRules
{
    public static ManualCareerPathProfileValidator DefaultManualCareerPathProfileValidator()
    {
        return new ManualCareerPathProfileValidator(
            new List<IManualCareerPathProfileRule>
            {
                new ManualCareerPathProfileRetriesRangeRule(32, 86400),
                new ManualCareerPathProfileNameLengthRule(1000),
                new ManualCareerPathProfileRequiredCategoryRule("manual career path profile needs an entry"),
                new ManualCareerPathProfileChannelLengthRule(3600),
                new ManualCareerPathProfileCategoryLengthRule(86400),
            });
    }
}
