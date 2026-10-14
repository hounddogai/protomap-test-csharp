namespace Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive benchmark notice must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveBenchmarkNoticeRule
{
    List<string> Check(AdaptiveBenchmarkNotice item);
}

public sealed record AdaptiveBenchmarkNoticeScoreRangeRule(double Min, double Max) : IAdaptiveBenchmarkNoticeRule
{
    public List<string> Check(AdaptiveBenchmarkNotice item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveBenchmarkNoticeRequiredCodeRule(string Message) : IAdaptiveBenchmarkNoticeRule
{
    public List<string> Check(AdaptiveBenchmarkNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBenchmarkNoticeCostCenterLengthRule(int MaxLength) : IAdaptiveBenchmarkNoticeRule
{
    public List<string> Check(AdaptiveBenchmarkNotice item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBenchmarkNoticeAllowedManagerIdRule(List<string> Allowed) : IAdaptiveBenchmarkNoticeRule
{
    public List<string> Check(AdaptiveBenchmarkNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is expired" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBenchmarkNoticeCodeLengthRule(int MaxLength) : IAdaptiveBenchmarkNoticeRule
{
    public List<string> Check(AdaptiveBenchmarkNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBenchmarkNoticeRequiredCostCenterRule(string Message) : IAdaptiveBenchmarkNoticeRule
{
    public List<string> Check(AdaptiveBenchmarkNotice item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive benchmark notice.</summary>
public sealed record AdaptiveBenchmarkNoticeValidator(List<IAdaptiveBenchmarkNoticeRule> Rules)
{
    public List<string> Validate(AdaptiveBenchmarkNotice item)
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

    public void RequireValid(AdaptiveBenchmarkNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveBenchmarkNotice> items)
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

/// <summary>Functions of adaptive benchmark notice rules.</summary>
public static class AdaptiveBenchmarkNoticeRules
{
    public static AdaptiveBenchmarkNoticeValidator DefaultAdaptiveBenchmarkNoticeValidator()
    {
        return new AdaptiveBenchmarkNoticeValidator(
            new List<IAdaptiveBenchmarkNoticeRule>
            {
                new AdaptiveBenchmarkNoticeScoreRangeRule(9.5, 853.9),
                new AdaptiveBenchmarkNoticeRequiredCodeRule("adaptive benchmark notice needs an entry"),
                new AdaptiveBenchmarkNoticeCostCenterLengthRule(86400),
                new AdaptiveBenchmarkNoticeAllowedManagerIdRule(new List<string>()),
                new AdaptiveBenchmarkNoticeCodeLengthRule(3600),
                new AdaptiveBenchmarkNoticeRequiredCostCenterRule("adaptive benchmark notice needs a value"),
            });
    }
}
