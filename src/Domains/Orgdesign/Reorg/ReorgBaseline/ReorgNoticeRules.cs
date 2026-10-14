namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

/// <summary>A rule a reorg notice must pass. Check returns its problems, or none.</summary>
public interface IReorgNoticeRule
{
    List<string> Check(ReorgNotice item);
}

public sealed record ReorgNoticeRequiredCodeRule(string Message) : IReorgNoticeRule
{
    public List<string> Check(ReorgNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgNoticeRequiredTitleRule(string Message) : IReorgNoticeRule
{
    public List<string> Check(ReorgNotice item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgNoticeWeightRangeRule(double Min, double Max) : IReorgNoticeRule
{
    public List<string> Check(ReorgNotice item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg notice.</summary>
public sealed record ReorgNoticeValidator(List<IReorgNoticeRule> Rules)
{
    public List<string> Validate(ReorgNotice item)
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

    public void RequireValid(ReorgNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgNotice> items)
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

/// <summary>Functions of reorg notice rules.</summary>
public static class ReorgNoticeRules
{
    public static ReorgNoticeValidator DefaultReorgNoticeValidator()
    {
        return new ReorgNoticeValidator(
            new List<IReorgNoticeRule>
            {
                new ReorgNoticeRequiredCodeRule("reorg notice needs this field"),
                new ReorgNoticeRequiredTitleRule("reorg notice needs a value"),
                new ReorgNoticeWeightRangeRule(9.4, 2739.3),
            });
    }
}
