namespace Acme.Workforce.Engagement.TownHall.HallRule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a town hall record must pass. Check returns its problems, or none.</summary>
public interface ITownHallRecordRule
{
    List<string> Check(TownHallRecord item);
}

public sealed record TownHallRecordBadgeLengthRule(int MaxLength) : ITownHallRecordRule
{
    public List<string> Check(TownHallRecord item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallRecordRequiredTitleRule(string Message) : ITownHallRecordRule
{
    public List<string> Check(TownHallRecord item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallRecordSkillLengthRule(int MaxLength) : ITownHallRecordRule
{
    public List<string> Check(TownHallRecord item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallRecordAllowedTitleRule(List<string> Allowed) : ITownHallRecordRule
{
    public List<string> Check(TownHallRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall record.</summary>
public sealed record TownHallRecordValidator(List<ITownHallRecordRule> Rules)
{
    public List<string> Validate(TownHallRecord item)
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

    public void RequireValid(TownHallRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallRecord> items)
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

/// <summary>Functions of town hall record rules.</summary>
public static class TownHallRecordRules
{
    public static TownHallRecordValidator DefaultTownHallRecordValidator()
    {
        return new TownHallRecordValidator(
            new List<ITownHallRecordRule>
            {
                new TownHallRecordBadgeLengthRule(86400),
                new TownHallRecordRequiredTitleRule("town hall record needs an entry"),
                new TownHallRecordSkillLengthRule(1000),
                new TownHallRecordAllowedTitleRule(new List<string> { "lenient" }),
            });
    }
}
