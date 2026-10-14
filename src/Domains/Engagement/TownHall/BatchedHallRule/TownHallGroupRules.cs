namespace Acme.Workforce.Engagement.TownHall.BatchedHallRule;

/// <summary>A rule a town hall group must pass. Check returns its problems, or none.</summary>
public interface ITownHallGroupRule
{
    List<string> Check(TownHallGroup item);
}

public sealed record TownHallGroupCodeLengthRule(int MaxLength) : ITownHallGroupRule
{
    public List<string> Check(TownHallGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallGroupRequiredDepartmentRule(string Message) : ITownHallGroupRule
{
    public List<string> Check(TownHallGroup item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallGroupUpdatedAtRangeRule(int Min, int Max) : ITownHallGroupRule
{
    public List<string> Check(TownHallGroup item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallGroupDiscountRangeRule(double Min, double Max) : ITownHallGroupRule
{
    public List<string> Check(TownHallGroup item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a town hall group.</summary>
public sealed record TownHallGroupValidator(List<ITownHallGroupRule> Rules)
{
    public List<string> Validate(TownHallGroup item)
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

    public void RequireValid(TownHallGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallGroup> items)
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

/// <summary>Functions of town hall group rules.</summary>
public static class TownHallGroupRules
{
    public static TownHallGroupValidator DefaultTownHallGroupValidator()
    {
        return new TownHallGroupValidator(
            new List<ITownHallGroupRule>
            {
                new TownHallGroupCodeLengthRule(3600),
                new TownHallGroupRequiredDepartmentRule("town hall group needs an entry"),
                new TownHallGroupUpdatedAtRangeRule(16, 255),
                new TownHallGroupDiscountRangeRule(3.4, 1919.6),
            });
    }
}
