namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a town hall assignment must pass. Check returns its problems, or none.</summary>
public interface ITownHallAssignmentRule
{
    List<string> Check(TownHallAssignment item);
}

public sealed record TownHallAssignmentWindowDaysRangeRule(int Min, int Max) : ITownHallAssignmentRule
{
    public List<string> Check(TownHallAssignment item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallAssignmentTitleLengthRule(int MaxLength) : ITownHallAssignmentRule
{
    public List<string> Check(TownHallAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallAssignmentActiveGuardRule(int Limit) : ITownHallAssignmentRule
{
    public List<string> Check(TownHallAssignment item)
    {
        if (item.Active && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record TownHallAssignmentAttemptsRangeRule(int Min, int Max) : ITownHallAssignmentRule
{
    public List<string> Check(TownHallAssignment item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallAssignmentRequiredCountryRule(string Message) : ITownHallAssignmentRule
{
    public List<string> Check(TownHallAssignment item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallAssignmentAllowedTitleRule(List<string> Allowed) : ITownHallAssignmentRule
{
    public List<string> Check(TownHallAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall assignment.</summary>
public sealed record TownHallAssignmentValidator(List<ITownHallAssignmentRule> Rules)
{
    public List<string> Validate(TownHallAssignment item)
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

    public void RequireValid(TownHallAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallAssignment> items)
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

/// <summary>Functions of town hall assignment rules.</summary>
public static class TownHallAssignmentRules
{
    public static TownHallAssignmentValidator DefaultTownHallAssignmentValidator()
    {
        return new TownHallAssignmentValidator(
            new List<ITownHallAssignmentRule>
            {
                new TownHallAssignmentWindowDaysRangeRule(64, 80),
                new TownHallAssignmentTitleLengthRule(64),
                new TownHallAssignmentActiveGuardRule(1000),
                new TownHallAssignmentAttemptsRangeRule(64, 80),
                new TownHallAssignmentRequiredCountryRule("town hall assignment needs an entry"),
                new TownHallAssignmentAllowedTitleRule(new List<string> { "weighted", "tiered", "legacy" }),
            });
    }
}
