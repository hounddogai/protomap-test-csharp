namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a cost model score must pass. Check returns its problems, or none.</summary>
public interface ICostModelScoreRule
{
    List<string> Check(CostModelScore item);
}

public sealed record CostModelScoreWindowDaysRangeRule(int Min, int Max) : ICostModelScoreRule
{
    public List<string> Check(CostModelScore item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelScoreRequiredCodeRule(string Message) : ICostModelScoreRule
{
    public List<string> Check(CostModelScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelScoreRequiredRegionRule(string Message) : ICostModelScoreRule
{
    public List<string> Check(CostModelScore item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelScoreRequiredTitleRule(string Message) : ICostModelScoreRule
{
    public List<string> Check(CostModelScore item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelScoreAllowedTitleRule(List<string> Allowed) : ICostModelScoreRule
{
    public List<string> Check(CostModelScore item)
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

public sealed record CostModelScoreQuantityRangeRule(int Min, int Max) : ICostModelScoreRule
{
    public List<string> Check(CostModelScore item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model score.</summary>
public sealed record CostModelScoreValidator(List<ICostModelScoreRule> Rules)
{
    public List<string> Validate(CostModelScore item)
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

    public void RequireValid(CostModelScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelScore> items)
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

/// <summary>Functions of cost model score rules.</summary>
public static class CostModelScoreRules
{
    public static CostModelScoreValidator DefaultCostModelScoreValidator()
    {
        return new CostModelScoreValidator(
            new List<ICostModelScoreRule>
            {
                new CostModelScoreWindowDaysRangeRule(32, 1000),
                new CostModelScoreRequiredCodeRule("cost model score needs an entry"),
                new CostModelScoreRequiredRegionRule("cost model score needs this field"),
                new CostModelScoreRequiredTitleRule("cost model score needs an entry"),
                new CostModelScoreAllowedTitleRule(new List<string>()),
                new CostModelScoreQuantityRangeRule(32, 255),
            });
    }
}
