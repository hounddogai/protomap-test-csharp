namespace Acme.Workforce.Workplace.Visitor.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling visitor window must pass. Check returns its problems, or none.</summary>
public interface IRollingVisitorWindowRule
{
    List<string> Check(RollingVisitorWindow item);
}

public sealed record RollingVisitorWindowRequiredChannelRule(string Message) : IRollingVisitorWindowRule
{
    public List<string> Check(RollingVisitorWindow item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingVisitorWindowRevisionRangeRule(int Min, int Max) : IRollingVisitorWindowRule
{
    public List<string> Check(RollingVisitorWindow item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingVisitorWindowChannelLengthRule(int MaxLength) : IRollingVisitorWindowRule
{
    public List<string> Check(RollingVisitorWindow item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingVisitorWindowAllowedNameRule(List<string> Allowed) : IRollingVisitorWindowRule
{
    public List<string> Check(RollingVisitorWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling visitor window.</summary>
public sealed record RollingVisitorWindowValidator(List<IRollingVisitorWindowRule> Rules)
{
    public List<string> Validate(RollingVisitorWindow item)
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

    public void RequireValid(RollingVisitorWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingVisitorWindow> items)
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

/// <summary>Functions of rolling visitor window rules.</summary>
public static class RollingVisitorWindowRules
{
    public static RollingVisitorWindowValidator DefaultRollingVisitorWindowValidator()
    {
        return new RollingVisitorWindowValidator(
            new List<IRollingVisitorWindowRule>
            {
                new RollingVisitorWindowRequiredChannelRule("rolling visitor window needs this field"),
                new RollingVisitorWindowRevisionRangeRule(1000, 86400),
                new RollingVisitorWindowChannelLengthRule(120),
                new RollingVisitorWindowAllowedNameRule(new List<string> { "regional" }),
            });
    }
}
