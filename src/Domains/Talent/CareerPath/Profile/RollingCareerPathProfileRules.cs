namespace Acme.Workforce.Talent.CareerPath.Profile;

/// <summary>A rule a rolling career path profile must pass. Check returns its problems, or none.</summary>
public interface IRollingCareerPathProfileRule
{
    List<string> Check(RollingCareerPathProfile item);
}

public sealed record RollingCareerPathProfileTimezoneLengthRule(int MaxLength) : IRollingCareerPathProfileRule
{
    public List<string> Check(RollingCareerPathProfile item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingCareerPathProfileRequiredJobCodeRule(string Message) : IRollingCareerPathProfileRule
{
    public List<string> Check(RollingCareerPathProfile item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingCareerPathProfileCodePrefixRule(string Prefix) : IRollingCareerPathProfileRule
{
    public List<string> Check(RollingCareerPathProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingCareerPathProfileDurationMinutesRangeRule(int Min, int Max) : IRollingCareerPathProfileRule
{
    public List<string> Check(RollingCareerPathProfile item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling career path profile.</summary>
public sealed record RollingCareerPathProfileValidator(List<IRollingCareerPathProfileRule> Rules)
{
    public List<string> Validate(RollingCareerPathProfile item)
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

    public void RequireValid(RollingCareerPathProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingCareerPathProfile> items)
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

/// <summary>Functions of rolling career path profile rules.</summary>
public static class RollingCareerPathProfileRules
{
    public static RollingCareerPathProfileValidator DefaultRollingCareerPathProfileValidator()
    {
        return new RollingCareerPathProfileValidator(
            new List<IRollingCareerPathProfileRule>
            {
                new RollingCareerPathProfileTimezoneLengthRule(80),
                new RollingCareerPathProfileRequiredJobCodeRule("rolling career path profile needs a value"),
                new RollingCareerPathProfileCodePrefixRule("X"),
                new RollingCareerPathProfileDurationMinutesRangeRule(16, 80),
            });
    }
}
