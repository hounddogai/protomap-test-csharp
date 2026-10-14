namespace Acme.Workforce.Compensation.MeritCycle.Profile;

/// <summary>A rule a merit cycle profile must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleProfileRule
{
    List<string> Check(MeritCycleProfile item);
}

public sealed record MeritCycleProfileCodePrefixRule(string Prefix) : IMeritCycleProfileRule
{
    public List<string> Check(MeritCycleProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleProfileReviewerLengthRule(int MaxLength) : IMeritCycleProfileRule
{
    public List<string> Check(MeritCycleProfile item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleProfileRegionLengthRule(int MaxLength) : IMeritCycleProfileRule
{
    public List<string> Check(MeritCycleProfile item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleProfileRequiredTitleRule(string Message) : IMeritCycleProfileRule
{
    public List<string> Check(MeritCycleProfile item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle profile.</summary>
public sealed record MeritCycleProfileValidator(List<IMeritCycleProfileRule> Rules)
{
    public List<string> Validate(MeritCycleProfile item)
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

    public void RequireValid(MeritCycleProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleProfile> items)
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

/// <summary>Functions of merit cycle profile rules.</summary>
public static class MeritCycleProfileRules
{
    public static MeritCycleProfileValidator DefaultMeritCycleProfileValidator()
    {
        return new MeritCycleProfileValidator(
            new List<IMeritCycleProfileRule>
            {
                new MeritCycleProfileCodePrefixRule("OPS"),
                new MeritCycleProfileReviewerLengthRule(255),
                new MeritCycleProfileRegionLengthRule(120),
                new MeritCycleProfileRequiredTitleRule("merit cycle profile needs an entry"),
            });
    }
}
