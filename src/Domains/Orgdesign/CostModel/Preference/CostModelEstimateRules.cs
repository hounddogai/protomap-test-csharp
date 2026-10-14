namespace Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>A rule a cost model estimate must pass. Check returns its problems, or none.</summary>
public interface ICostModelEstimateRule
{
    List<string> Check(CostModelEstimate item);
}

public sealed record CostModelEstimateRequiredSummaryRule(string Message) : ICostModelEstimateRule
{
    public List<string> Check(CostModelEstimate item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelEstimateTitleLengthRule(int MaxLength) : ICostModelEstimateRule
{
    public List<string> Check(CostModelEstimate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelEstimateCapacityRangeRule(int Min, int Max) : ICostModelEstimateRule
{
    public List<string> Check(CostModelEstimate item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CostModelEstimateWindowDaysRangeRule(int Min, int Max) : ICostModelEstimateRule
{
    public List<string> Check(CostModelEstimate item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelEstimateAttemptsRangeRule(int Min, int Max) : ICostModelEstimateRule
{
    public List<string> Check(CostModelEstimate item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelEstimateSummaryLengthRule(int MaxLength) : ICostModelEstimateRule
{
    public List<string> Check(CostModelEstimate item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model estimate.</summary>
public sealed record CostModelEstimateValidator(List<ICostModelEstimateRule> Rules)
{
    public List<string> Validate(CostModelEstimate item)
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

    public void RequireValid(CostModelEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelEstimate> items)
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

/// <summary>Functions of cost model estimate rules.</summary>
public static class CostModelEstimateRules
{
    public static CostModelEstimateValidator DefaultCostModelEstimateValidator()
    {
        return new CostModelEstimateValidator(
            new List<ICostModelEstimateRule>
            {
                new CostModelEstimateRequiredSummaryRule("cost model estimate needs a value"),
                new CostModelEstimateTitleLengthRule(3600),
                new CostModelEstimateCapacityRangeRule(86400, 86400),
                new CostModelEstimateWindowDaysRangeRule(120, 3600),
                new CostModelEstimateAttemptsRangeRule(1000, 86400),
                new CostModelEstimateSummaryLengthRule(86400),
            });
    }
}
