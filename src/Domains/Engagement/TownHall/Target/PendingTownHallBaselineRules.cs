namespace Acme.Workforce.Engagement.TownHall.Target;

/// <summary>A rule a pending town hall baseline must pass. Check returns its problems, or none.</summary>
public interface IPendingTownHallBaselineRule
{
    List<string> Check(PendingTownHallBaseline item);
}

public sealed record PendingTownHallBaselineTeamLengthRule(int MaxLength) : IPendingTownHallBaselineRule
{
    public List<string> Check(PendingTownHallBaseline item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingTownHallBaselineMarginRangeRule(double Min, double Max) : IPendingTownHallBaselineRule
{
    public List<string> Check(PendingTownHallBaseline item)
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

public sealed record PendingTownHallBaselineRequiredTeamRule(string Message) : IPendingTownHallBaselineRule
{
    public List<string> Check(PendingTownHallBaseline item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingTownHallBaselineRequiredChannelRule(string Message) : IPendingTownHallBaselineRule
{
    public List<string> Check(PendingTownHallBaseline item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingTownHallBaselineCodeLengthRule(int MaxLength) : IPendingTownHallBaselineRule
{
    public List<string> Check(PendingTownHallBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending town hall baseline.</summary>
public sealed record PendingTownHallBaselineValidator(List<IPendingTownHallBaselineRule> Rules)
{
    public List<string> Validate(PendingTownHallBaseline item)
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

    public void RequireValid(PendingTownHallBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingTownHallBaseline> items)
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

/// <summary>Functions of pending town hall baseline rules.</summary>
public static class PendingTownHallBaselineRules
{
    public static PendingTownHallBaselineValidator DefaultPendingTownHallBaselineValidator()
    {
        return new PendingTownHallBaselineValidator(
            new List<IPendingTownHallBaselineRule>
            {
                new PendingTownHallBaselineTeamLengthRule(1000),
                new PendingTownHallBaselineMarginRangeRule(9.6, 1247.7),
                new PendingTownHallBaselineRequiredTeamRule("pending town hall baseline needs an entry"),
                new PendingTownHallBaselineRequiredChannelRule("pending town hall baseline needs an entry"),
                new PendingTownHallBaselineCodeLengthRule(80),
            });
    }
}
