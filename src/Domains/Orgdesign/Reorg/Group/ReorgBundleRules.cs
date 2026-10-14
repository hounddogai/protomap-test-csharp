namespace Acme.Workforce.Orgdesign.Reorg.Group;

/// <summary>A rule a reorg bundle must pass. Check returns its problems, or none.</summary>
public interface IReorgBundleRule
{
    List<string> Check(ReorgBundle item);
}

public sealed record ReorgBundleRequiredCodeRule(string Message) : IReorgBundleRule
{
    public List<string> Check(ReorgBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgBundleRequiredTeamRule(string Message) : IReorgBundleRule
{
    public List<string> Check(ReorgBundle item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgBundleRequiredTitleRule(string Message) : IReorgBundleRule
{
    public List<string> Check(ReorgBundle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgBundleTeamLengthRule(int MaxLength) : IReorgBundleRule
{
    public List<string> Check(ReorgBundle item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgBundleChannelLengthRule(int MaxLength) : IReorgBundleRule
{
    public List<string> Check(ReorgBundle item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg bundle.</summary>
public sealed record ReorgBundleValidator(List<IReorgBundleRule> Rules)
{
    public List<string> Validate(ReorgBundle item)
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

    public void RequireValid(ReorgBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgBundle> items)
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

/// <summary>Functions of reorg bundle rules.</summary>
public static class ReorgBundleRules
{
    public static ReorgBundleValidator DefaultReorgBundleValidator()
    {
        return new ReorgBundleValidator(
            new List<IReorgBundleRule>
            {
                new ReorgBundleRequiredCodeRule("reorg bundle needs this field"),
                new ReorgBundleRequiredTeamRule("reorg bundle needs this field"),
                new ReorgBundleRequiredTitleRule("reorg bundle needs this field"),
                new ReorgBundleTeamLengthRule(1000),
                new ReorgBundleChannelLengthRule(64),
            });
    }
}
