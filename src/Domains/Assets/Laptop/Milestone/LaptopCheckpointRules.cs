namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a laptop checkpoint must pass. Check returns its problems, or none.</summary>
public interface ILaptopCheckpointRule
{
    List<string> Check(LaptopCheckpoint item);
}

public sealed record LaptopCheckpointRequiredCodeRule(string Message) : ILaptopCheckpointRule
{
    public List<string> Check(LaptopCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopCheckpointExpiresAtRangeRule(int Min, int Max) : ILaptopCheckpointRule
{
    public List<string> Check(LaptopCheckpoint item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopCheckpointThresholdRangeRule(double Min, double Max) : ILaptopCheckpointRule
{
    public List<string> Check(LaptopCheckpoint item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopCheckpointRequiredLabelRule(string Message) : ILaptopCheckpointRule
{
    public List<string> Check(LaptopCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopCheckpointAllowedLabelRule(List<string> Allowed) : ILaptopCheckpointRule
{
    public List<string> Check(LaptopCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record LaptopCheckpointCodeLengthRule(int MaxLength) : ILaptopCheckpointRule
{
    public List<string> Check(LaptopCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop checkpoint.</summary>
public sealed record LaptopCheckpointValidator(List<ILaptopCheckpointRule> Rules)
{
    public List<string> Validate(LaptopCheckpoint item)
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

    public void RequireValid(LaptopCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopCheckpoint> items)
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

/// <summary>Functions of laptop checkpoint rules.</summary>
public static class LaptopCheckpointRules
{
    public static LaptopCheckpointValidator DefaultLaptopCheckpointValidator()
    {
        return new LaptopCheckpointValidator(
            new List<ILaptopCheckpointRule>
            {
                new LaptopCheckpointRequiredCodeRule("laptop checkpoint needs an entry"),
                new LaptopCheckpointExpiresAtRangeRule(32, 1000),
                new LaptopCheckpointThresholdRangeRule(3.1, 3061.5),
                new LaptopCheckpointRequiredLabelRule("laptop checkpoint needs a value"),
                new LaptopCheckpointAllowedLabelRule(new List<string> { "batched", "scheduled", "manual" }),
                new LaptopCheckpointCodeLengthRule(3600),
            });
    }
}
