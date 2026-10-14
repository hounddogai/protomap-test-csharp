namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>A rule a strict cost model estimate must pass. Check returns its problems, or none.</summary>
public interface IStrictCostModelEstimateRule
{
    List<string> Check(StrictCostModelEstimate item);
}

public sealed record StrictCostModelEstimateTitleLengthRule(int MaxLength) : IStrictCostModelEstimateRule
{
    public List<string> Check(StrictCostModelEstimate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictCostModelEstimateRequiredPhoneRule(string Message) : IStrictCostModelEstimateRule
{
    public List<string> Check(StrictCostModelEstimate item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictCostModelEstimateCodeLengthRule(int MaxLength) : IStrictCostModelEstimateRule
{
    public List<string> Check(StrictCostModelEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict cost model estimate.</summary>
public sealed record StrictCostModelEstimateValidator(List<IStrictCostModelEstimateRule> Rules)
{
    public List<string> Validate(StrictCostModelEstimate item)
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

    public void RequireValid(StrictCostModelEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictCostModelEstimate> items)
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

/// <summary>Functions of strict cost model estimate rules.</summary>
public static class StrictCostModelEstimateRules
{
    public static StrictCostModelEstimateValidator DefaultStrictCostModelEstimateValidator()
    {
        return new StrictCostModelEstimateValidator(
            new List<IStrictCostModelEstimateRule>
            {
                new StrictCostModelEstimateTitleLengthRule(16),
                new StrictCostModelEstimateRequiredPhoneRule("strict cost model estimate needs this field"),
                new StrictCostModelEstimateCodeLengthRule(86400),
            });
    }
}
