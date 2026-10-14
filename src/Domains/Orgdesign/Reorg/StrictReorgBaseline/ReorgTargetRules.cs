namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>A rule a reorg target must pass. Check returns its problems, or none.</summary>
public interface IReorgTargetRule
{
    List<string> Check(ReorgTarget item);
}

public sealed record ReorgTargetRetriesRangeRule(int Min, int Max) : IReorgTargetRule
{
    public List<string> Check(ReorgTarget item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ReorgTargetRequiredJobCodeRule(string Message) : IReorgTargetRule
{
    public List<string> Check(ReorgTarget item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgTargetSequenceRangeRule(int Min, int Max) : IReorgTargetRule
{
    public List<string> Check(ReorgTarget item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ReorgTargetTitleLengthRule(int MaxLength) : IReorgTargetRule
{
    public List<string> Check(ReorgTarget item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgTargetBadgeLengthRule(int MaxLength) : IReorgTargetRule
{
    public List<string> Check(ReorgTarget item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg target.</summary>
public sealed record ReorgTargetValidator(List<IReorgTargetRule> Rules)
{
    public List<string> Validate(ReorgTarget item)
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

    public void RequireValid(ReorgTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgTarget> items)
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

/// <summary>Functions of reorg target rules.</summary>
public static class ReorgTargetRules
{
    public static ReorgTargetValidator DefaultReorgTargetValidator()
    {
        return new ReorgTargetValidator(
            new List<IReorgTargetRule>
            {
                new ReorgTargetRetriesRangeRule(16, 80),
                new ReorgTargetRequiredJobCodeRule("reorg target needs this field"),
                new ReorgTargetSequenceRangeRule(16, 32),
                new ReorgTargetTitleLengthRule(86400),
                new ReorgTargetBadgeLengthRule(16),
            });
    }
}
