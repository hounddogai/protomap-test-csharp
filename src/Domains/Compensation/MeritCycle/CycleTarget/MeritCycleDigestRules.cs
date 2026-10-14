namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

/// <summary>A rule a merit cycle digest must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleDigestRule
{
    List<string> Check(MeritCycleDigest item);
}

public sealed record MeritCycleDigestTagsLimitRule(int MaxCount, List<string> Banned) : IMeritCycleDigestRule
{
    public List<string> Check(MeritCycleDigest item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record MeritCycleDigestDescriptionLengthRule(int MaxLength) : IMeritCycleDigestRule
{
    public List<string> Check(MeritCycleDigest item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleDigestCodeLengthRule(int MaxLength) : IMeritCycleDigestRule
{
    public List<string> Check(MeritCycleDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleDigestWeightRangeRule(double Min, double Max) : IMeritCycleDigestRule
{
    public List<string> Check(MeritCycleDigest item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a merit cycle digest.</summary>
public sealed record MeritCycleDigestValidator(List<IMeritCycleDigestRule> Rules)
{
    public List<string> Validate(MeritCycleDigest item)
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

    public void RequireValid(MeritCycleDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleDigest> items)
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

/// <summary>Functions of merit cycle digest rules.</summary>
public static class MeritCycleDigestRules
{
    public static MeritCycleDigestValidator DefaultMeritCycleDigestValidator()
    {
        return new MeritCycleDigestValidator(
            new List<IMeritCycleDigestRule>
            {
                new MeritCycleDigestTagsLimitRule(3600, new List<string>()),
                new MeritCycleDigestDescriptionLengthRule(80),
                new MeritCycleDigestCodeLengthRule(1000),
                new MeritCycleDigestWeightRangeRule(9.8, 248.1),
            });
    }
}
