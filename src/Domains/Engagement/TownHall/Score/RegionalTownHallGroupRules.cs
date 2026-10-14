namespace Acme.Workforce.Engagement.TownHall.Score;

/// <summary>A rule a regional town hall group must pass. Check returns its problems, or none.</summary>
public interface IRegionalTownHallGroupRule
{
    List<string> Check(RegionalTownHallGroup item);
}

public sealed record RegionalTownHallGroupRequiredNameRule(string Message) : IRegionalTownHallGroupRule
{
    public List<string> Check(RegionalTownHallGroup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalTownHallGroupDurationMinutesRangeRule(int Min, int Max) : IRegionalTownHallGroupRule
{
    public List<string> Check(RegionalTownHallGroup item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTownHallGroupCodeLengthRule(int MaxLength) : IRegionalTownHallGroupRule
{
    public List<string> Check(RegionalTownHallGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTownHallGroupRequiredLabelRule(string Message) : IRegionalTownHallGroupRule
{
    public List<string> Check(RegionalTownHallGroup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalTownHallGroupBadgeLengthRule(int MaxLength) : IRegionalTownHallGroupRule
{
    public List<string> Check(RegionalTownHallGroup item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional town hall group.</summary>
public sealed record RegionalTownHallGroupValidator(List<IRegionalTownHallGroupRule> Rules)
{
    public List<string> Validate(RegionalTownHallGroup item)
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

    public void RequireValid(RegionalTownHallGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalTownHallGroup> items)
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

/// <summary>Functions of regional town hall group rules.</summary>
public static class RegionalTownHallGroupRules
{
    public static RegionalTownHallGroupValidator DefaultRegionalTownHallGroupValidator()
    {
        return new RegionalTownHallGroupValidator(
            new List<IRegionalTownHallGroupRule>
            {
                new RegionalTownHallGroupRequiredNameRule("regional town hall group needs a value"),
                new RegionalTownHallGroupDurationMinutesRangeRule(64, 64),
                new RegionalTownHallGroupCodeLengthRule(3600),
                new RegionalTownHallGroupRequiredLabelRule("regional town hall group needs a value"),
                new RegionalTownHallGroupBadgeLengthRule(80),
            });
    }
}
