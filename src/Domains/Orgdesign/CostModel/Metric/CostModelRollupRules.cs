namespace Acme.Workforce.Orgdesign.CostModel.Metric;

/// <summary>A rule a cost model rollup must pass. Check returns its problems, or none.</summary>
public interface ICostModelRollupRule
{
    List<string> Check(CostModelRollup item);
}

public sealed record CostModelRollupLabelLengthRule(int MaxLength) : ICostModelRollupRule
{
    public List<string> Check(CostModelRollup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelRollupStartsAtRangeRule(int Min, int Max) : ICostModelRollupRule
{
    public List<string> Check(CostModelRollup item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CostModelRollupChannelLengthRule(int MaxLength) : ICostModelRollupRule
{
    public List<string> Check(CostModelRollup item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelRollupRequiredChannelRule(string Message) : ICostModelRollupRule
{
    public List<string> Check(CostModelRollup item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelRollupRequiredLabelRule(string Message) : ICostModelRollupRule
{
    public List<string> Check(CostModelRollup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelRollupUnitLengthRule(int MaxLength) : ICostModelRollupRule
{
    public List<string> Check(CostModelRollup item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model rollup.</summary>
public sealed record CostModelRollupValidator(List<ICostModelRollupRule> Rules)
{
    public List<string> Validate(CostModelRollup item)
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

    public void RequireValid(CostModelRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelRollup> items)
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

/// <summary>Functions of cost model rollup rules.</summary>
public static class CostModelRollupRules
{
    public static CostModelRollupValidator DefaultCostModelRollupValidator()
    {
        return new CostModelRollupValidator(
            new List<ICostModelRollupRule>
            {
                new CostModelRollupLabelLengthRule(1000),
                new CostModelRollupStartsAtRangeRule(16, 1000),
                new CostModelRollupChannelLengthRule(255),
                new CostModelRollupRequiredChannelRule("cost model rollup needs this field"),
                new CostModelRollupRequiredLabelRule("cost model rollup needs this field"),
                new CostModelRollupUnitLengthRule(32),
            });
    }
}
