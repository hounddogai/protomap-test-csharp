namespace Acme.Workforce.Talent.Mobility.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a mobility profile must pass. Check returns its problems, or none.</summary>
public interface IMobilityProfileRule
{
    List<string> Check(MobilityProfile item);
}

public sealed record MobilityProfilePriorityRangeRule(int Min, int Max) : IMobilityProfileRule
{
    public List<string> Check(MobilityProfile item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MobilityProfileRequiredCodeRule(string Message) : IMobilityProfileRule
{
    public List<string> Check(MobilityProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityProfileAllowedCodeRule(List<string> Allowed) : IMobilityProfileRule
{
    public List<string> Check(MobilityProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record MobilityProfileFteRangeRule(double Min, double Max) : IMobilityProfileRule
{
    public List<string> Check(MobilityProfile item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a mobility profile.</summary>
public sealed record MobilityProfileValidator(List<IMobilityProfileRule> Rules)
{
    public List<string> Validate(MobilityProfile item)
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

    public void RequireValid(MobilityProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityProfile> items)
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

/// <summary>Functions of mobility profile rules.</summary>
public static class MobilityProfileRules
{
    public static MobilityProfileValidator DefaultMobilityProfileValidator()
    {
        return new MobilityProfileValidator(
            new List<IMobilityProfileRule>
            {
                new MobilityProfilePriorityRangeRule(80, 1000),
                new MobilityProfileRequiredCodeRule("mobility profile needs a value"),
                new MobilityProfileAllowedCodeRule(new List<string> { "rolling", "scheduled" }),
                new MobilityProfileFteRangeRule(5.7, 3189.9),
            });
    }
}
