namespace Acme.Workforce.Workplace.Visitor.OverrideItem;

/// <summary>A rule a visitor override must pass. Check returns its problems, or none.</summary>
public interface IVisitorOverrideRule
{
    List<string> Check(VisitorOverride item);
}

public sealed record VisitorOverrideNameLengthRule(int MaxLength) : IVisitorOverrideRule
{
    public List<string> Check(VisitorOverride item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorOverrideManagerIdLengthRule(int MaxLength) : IVisitorOverrideRule
{
    public List<string> Check(VisitorOverride item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorOverrideCodeLengthRule(int MaxLength) : IVisitorOverrideRule
{
    public List<string> Check(VisitorOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorOverrideRequiredCodeRule(string Message) : IVisitorOverrideRule
{
    public List<string> Check(VisitorOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorOverrideLeaveDaysRangeRule(int Min, int Max) : IVisitorOverrideRule
{
    public List<string> Check(VisitorOverride item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor override.</summary>
public sealed record VisitorOverrideValidator(List<IVisitorOverrideRule> Rules)
{
    public List<string> Validate(VisitorOverride item)
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

    public void RequireValid(VisitorOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorOverride> items)
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

/// <summary>Functions of visitor override rules.</summary>
public static class VisitorOverrideRules
{
    public static VisitorOverrideValidator DefaultVisitorOverrideValidator()
    {
        return new VisitorOverrideValidator(
            new List<IVisitorOverrideRule>
            {
                new VisitorOverrideNameLengthRule(86400),
                new VisitorOverrideManagerIdLengthRule(86400),
                new VisitorOverrideCodeLengthRule(3600),
                new VisitorOverrideRequiredCodeRule("visitor override needs a value"),
                new VisitorOverrideLeaveDaysRangeRule(16, 120),
            });
    }
}
