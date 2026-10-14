namespace Acme.Workforce.Assets.Laptop.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive laptop checkpoint must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveLaptopCheckpointRule
{
    List<string> Check(AdaptiveLaptopCheckpoint item);
}

public sealed record AdaptiveLaptopCheckpointBudgetRangeRule(double Min, double Max) : IAdaptiveLaptopCheckpointRule
{
    public List<string> Check(AdaptiveLaptopCheckpoint item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveLaptopCheckpointAllowedLabelRule(List<string> Allowed) : IAdaptiveLaptopCheckpointRule
{
    public List<string> Check(AdaptiveLaptopCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is missing" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLaptopCheckpointCodeLengthRule(int MaxLength) : IAdaptiveLaptopCheckpointRule
{
    public List<string> Check(AdaptiveLaptopCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLaptopCheckpointRequiredLabelRule(string Message) : IAdaptiveLaptopCheckpointRule
{
    public List<string> Check(AdaptiveLaptopCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive laptop checkpoint.</summary>
public sealed record AdaptiveLaptopCheckpointValidator(List<IAdaptiveLaptopCheckpointRule> Rules)
{
    public List<string> Validate(AdaptiveLaptopCheckpoint item)
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

    public void RequireValid(AdaptiveLaptopCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveLaptopCheckpoint> items)
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

/// <summary>Functions of adaptive laptop checkpoint rules.</summary>
public static class AdaptiveLaptopCheckpointRules
{
    public static AdaptiveLaptopCheckpointValidator DefaultAdaptiveLaptopCheckpointValidator()
    {
        return new AdaptiveLaptopCheckpointValidator(
            new List<IAdaptiveLaptopCheckpointRule>
            {
                new AdaptiveLaptopCheckpointBudgetRangeRule(5.7, 4394.8),
                new AdaptiveLaptopCheckpointAllowedLabelRule(new List<string>()),
                new AdaptiveLaptopCheckpointCodeLengthRule(120),
                new AdaptiveLaptopCheckpointRequiredLabelRule("adaptive laptop checkpoint needs this field"),
            });
    }
}
