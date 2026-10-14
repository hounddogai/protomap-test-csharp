namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a reorg assignment must pass. Check returns its problems, or none.</summary>
public interface IReorgAssignmentRule
{
    List<string> Check(ReorgAssignment item);
}

public sealed record ReorgAssignmentRequiredTitleRule(string Message) : IReorgAssignmentRule
{
    public List<string> Check(ReorgAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgAssignmentRetriesRangeRule(int Min, int Max) : IReorgAssignmentRule
{
    public List<string> Check(ReorgAssignment item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgAssignmentGraceDaysRangeRule(int Min, int Max) : IReorgAssignmentRule
{
    public List<string> Check(ReorgAssignment item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgAssignmentMarginRangeRule(double Min, double Max) : IReorgAssignmentRule
{
    public List<string> Check(ReorgAssignment item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgAssignmentAllowedTitleRule(List<string> Allowed) : IReorgAssignmentRule
{
    public List<string> Check(ReorgAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record ReorgAssignmentTitleLengthRule(int MaxLength) : IReorgAssignmentRule
{
    public List<string> Check(ReorgAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg assignment.</summary>
public sealed record ReorgAssignmentValidator(List<IReorgAssignmentRule> Rules)
{
    public List<string> Validate(ReorgAssignment item)
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

    public void RequireValid(ReorgAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgAssignment> items)
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

/// <summary>Functions of reorg assignment rules.</summary>
public static class ReorgAssignmentRules
{
    public static ReorgAssignmentValidator DefaultReorgAssignmentValidator()
    {
        return new ReorgAssignmentValidator(
            new List<IReorgAssignmentRule>
            {
                new ReorgAssignmentRequiredTitleRule("reorg assignment needs an entry"),
                new ReorgAssignmentRetriesRangeRule(64, 1000),
                new ReorgAssignmentGraceDaysRangeRule(16, 86400),
                new ReorgAssignmentMarginRangeRule(0.6, 4545.5),
                new ReorgAssignmentAllowedTitleRule(new List<string> { "manual", "default" }),
                new ReorgAssignmentTitleLengthRule(255),
            });
    }
}
