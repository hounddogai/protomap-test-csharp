namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

/// <summary>A rule a license seat score must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatScoreRule
{
    List<string> Check(LicenseSeatScore item);
}

public sealed record LicenseSeatScoreTitleLengthRule(int MaxLength) : ILicenseSeatScoreRule
{
    public List<string> Check(LicenseSeatScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatScoreEndsAtRangeRule(int Min, int Max) : ILicenseSeatScoreRule
{
    public List<string> Check(LicenseSeatScore item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LicenseSeatScoreSkillLengthRule(int MaxLength) : ILicenseSeatScoreRule
{
    public List<string> Check(LicenseSeatScore item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatScoreRequiredTitleRule(string Message) : ILicenseSeatScoreRule
{
    public List<string> Check(LicenseSeatScore item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat score.</summary>
public sealed record LicenseSeatScoreValidator(List<ILicenseSeatScoreRule> Rules)
{
    public List<string> Validate(LicenseSeatScore item)
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

    public void RequireValid(LicenseSeatScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatScore> items)
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

/// <summary>Functions of license seat score rules.</summary>
public static class LicenseSeatScoreRules
{
    public static LicenseSeatScoreValidator DefaultLicenseSeatScoreValidator()
    {
        return new LicenseSeatScoreValidator(
            new List<ILicenseSeatScoreRule>
            {
                new LicenseSeatScoreTitleLengthRule(3600),
                new LicenseSeatScoreEndsAtRangeRule(80, 1000),
                new LicenseSeatScoreSkillLengthRule(3600),
                new LicenseSeatScoreRequiredTitleRule("license seat score needs an entry"),
            });
    }
}
