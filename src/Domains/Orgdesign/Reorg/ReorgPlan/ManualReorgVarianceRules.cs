namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>A rule a manual reorg variance must pass. Check returns its problems, or none.</summary>
public interface IManualReorgVarianceRule
{
    List<string> Check(ManualReorgVariance item);
}

public sealed record ManualReorgVarianceRequiredCostCenterRule(string Message) : IManualReorgVarianceRule
{
    public List<string> Check(ManualReorgVariance item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualReorgVarianceMaximumRangeRule(double Min, double Max) : IManualReorgVarianceRule
{
    public List<string> Check(ManualReorgVariance item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualReorgVarianceCodeLengthRule(int MaxLength) : IManualReorgVarianceRule
{
    public List<string> Check(ManualReorgVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualReorgVarianceCostCenterLengthRule(int MaxLength) : IManualReorgVarianceRule
{
    public List<string> Check(ManualReorgVariance item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualReorgVarianceFteRangeRule(double Min, double Max) : IManualReorgVarianceRule
{
    public List<string> Check(ManualReorgVariance item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a manual reorg variance.</summary>
public sealed record ManualReorgVarianceValidator(List<IManualReorgVarianceRule> Rules)
{
    public List<string> Validate(ManualReorgVariance item)
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

    public void RequireValid(ManualReorgVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualReorgVariance> items)
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

/// <summary>Functions of manual reorg variance rules.</summary>
public static class ManualReorgVarianceRules
{
    public static ManualReorgVarianceValidator DefaultManualReorgVarianceValidator()
    {
        return new ManualReorgVarianceValidator(
            new List<IManualReorgVarianceRule>
            {
                new ManualReorgVarianceRequiredCostCenterRule("manual reorg variance needs this field"),
                new ManualReorgVarianceMaximumRangeRule(6.1, 1473.3),
                new ManualReorgVarianceCodeLengthRule(16),
                new ManualReorgVarianceCostCenterLengthRule(1000),
                new ManualReorgVarianceFteRangeRule(3.1, 2678.4),
            });
    }
}
