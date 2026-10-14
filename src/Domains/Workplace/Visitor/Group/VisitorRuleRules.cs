namespace Acme.Workforce.Workplace.Visitor.Group;

/// <summary>A rule a visitor rule must pass. Check returns its problems, or none.</summary>
public interface IVisitorRuleRule
{
    List<string> Check(VisitorRule item);
}

public sealed record VisitorRuleBatchSizeRangeRule(int Min, int Max) : IVisitorRuleRule
{
    public List<string> Check(VisitorRule item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorRuleScoreRangeRule(double Min, double Max) : IVisitorRuleRule
{
    public List<string> Check(VisitorRule item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorRuleRequiredTitleRule(string Message) : IVisitorRuleRule
{
    public List<string> Check(VisitorRule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor rule.</summary>
public sealed record VisitorRuleValidator(List<IVisitorRuleRule> Rules)
{
    public List<string> Validate(VisitorRule item)
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

    public void RequireValid(VisitorRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorRule> items)
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

/// <summary>Functions of visitor rule rules.</summary>
public static class VisitorRuleRules
{
    public static VisitorRuleValidator DefaultVisitorRuleValidator()
    {
        return new VisitorRuleValidator(
            new List<IVisitorRuleRule>
            {
                new VisitorRuleBatchSizeRangeRule(16, 80),
                new VisitorRuleScoreRangeRule(3.8, 1119.9),
                new VisitorRuleRequiredTitleRule("visitor rule needs a value"),
            });
    }
}
