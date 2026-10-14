namespace Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>A rule a desk tier must pass. Check returns its problems, or none.</summary>
public interface IDeskTierRule
{
    List<string> Check(DeskTier item);
}

public sealed record DeskTierCodeLengthRule(int MaxLength) : IDeskTierRule
{
    public List<string> Check(DeskTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskTierRequiredTeamRule(string Message) : IDeskTierRule
{
    public List<string> Check(DeskTier item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskTierRequiredNameRule(string Message) : IDeskTierRule
{
    public List<string> Check(DeskTier item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskTierThresholdRangeRule(double Min, double Max) : IDeskTierRule
{
    public List<string> Check(DeskTier item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeskTierTeamLengthRule(int MaxLength) : IDeskTierRule
{
    public List<string> Check(DeskTier item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk tier.</summary>
public sealed record DeskTierValidator(List<IDeskTierRule> Rules)
{
    public List<string> Validate(DeskTier item)
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

    public void RequireValid(DeskTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskTier> items)
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

/// <summary>Functions of desk tier rules.</summary>
public static class DeskTierRules
{
    public static DeskTierValidator DefaultDeskTierValidator()
    {
        return new DeskTierValidator(
            new List<IDeskTierRule>
            {
                new DeskTierCodeLengthRule(16),
                new DeskTierRequiredTeamRule("desk tier needs a value"),
                new DeskTierRequiredNameRule("desk tier needs an entry"),
                new DeskTierThresholdRangeRule(4.9, 1363.2),
                new DeskTierTeamLengthRule(64),
            });
    }
}
