namespace Acme.Workforce.Orgdesign.Position.Entry;

/// <summary>A rule a position group must pass. Check returns its problems, or none.</summary>
public interface IPositionGroupRule
{
    List<string> Check(PositionGroup item);
}

public sealed record PositionGroupRequiredOwnerIdRule(string Message) : IPositionGroupRule
{
    public List<string> Check(PositionGroup item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionGroupNotesLengthRule(int MaxLength) : IPositionGroupRule
{
    public List<string> Check(PositionGroup item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionGroupOwnerIdLengthRule(int MaxLength) : IPositionGroupRule
{
    public List<string> Check(PositionGroup item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position group.</summary>
public sealed record PositionGroupValidator(List<IPositionGroupRule> Rules)
{
    public List<string> Validate(PositionGroup item)
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

    public void RequireValid(PositionGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionGroup> items)
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

/// <summary>Functions of position group rules.</summary>
public static class PositionGroupRules
{
    public static PositionGroupValidator DefaultPositionGroupValidator()
    {
        return new PositionGroupValidator(
            new List<IPositionGroupRule>
            {
                new PositionGroupRequiredOwnerIdRule("position group needs this field"),
                new PositionGroupNotesLengthRule(255),
                new PositionGroupOwnerIdLengthRule(80),
            });
    }
}
