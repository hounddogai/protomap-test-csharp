namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped cost model metric must pass. Check returns its problems, or none.</summary>
public interface ICappedCostModelMetricRule
{
    List<string> Check(CappedCostModelMetric item);
}

public sealed record CappedCostModelMetricRequiredBadgeRule(string Message) : ICappedCostModelMetricRule
{
    public List<string> Check(CappedCostModelMetric item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedCostModelMetricAllowedCodeRule(List<string> Allowed) : ICappedCostModelMetricRule
{
    public List<string> Check(CappedCostModelMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record CappedCostModelMetricRequiredPhoneRule(string Message) : ICappedCostModelMetricRule
{
    public List<string> Check(CappedCostModelMetric item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedCostModelMetricVersionRangeRule(int Min, int Max) : ICappedCostModelMetricRule
{
    public List<string> Check(CappedCostModelMetric item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedCostModelMetricCodeLengthRule(int MaxLength) : ICappedCostModelMetricRule
{
    public List<string> Check(CappedCostModelMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped cost model metric.</summary>
public sealed record CappedCostModelMetricValidator(List<ICappedCostModelMetricRule> Rules)
{
    public List<string> Validate(CappedCostModelMetric item)
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

    public void RequireValid(CappedCostModelMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedCostModelMetric> items)
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

/// <summary>Functions of capped cost model metric rules.</summary>
public static class CappedCostModelMetricRules
{
    public static CappedCostModelMetricValidator DefaultCappedCostModelMetricValidator()
    {
        return new CappedCostModelMetricValidator(
            new List<ICappedCostModelMetricRule>
            {
                new CappedCostModelMetricRequiredBadgeRule("capped cost model metric needs this field"),
                new CappedCostModelMetricAllowedCodeRule(new List<string> { "pending", "regional", "tiered" }),
                new CappedCostModelMetricRequiredPhoneRule("capped cost model metric needs an entry"),
                new CappedCostModelMetricVersionRangeRule(32, 3600),
                new CappedCostModelMetricCodeLengthRule(80),
            });
    }
}
