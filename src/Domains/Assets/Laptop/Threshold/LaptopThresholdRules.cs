namespace Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>A rule a laptop threshold must pass. Check returns its problems, or none.</summary>
public interface ILaptopThresholdRule
{
    List<string> Check(LaptopThreshold item);
}

public sealed record LaptopThresholdLabelLengthRule(int MaxLength) : ILaptopThresholdRule
{
    public List<string> Check(LaptopThreshold item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopThresholdDiscountRangeRule(double Min, double Max) : ILaptopThresholdRule
{
    public List<string> Check(LaptopThreshold item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LaptopThresholdEmployeeIdLengthRule(int MaxLength) : ILaptopThresholdRule
{
    public List<string> Check(LaptopThreshold item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopThresholdRequiredLabelRule(string Message) : ILaptopThresholdRule
{
    public List<string> Check(LaptopThreshold item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopThresholdSequenceRangeRule(int Min, int Max) : ILaptopThresholdRule
{
    public List<string> Check(LaptopThreshold item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop threshold.</summary>
public sealed record LaptopThresholdValidator(List<ILaptopThresholdRule> Rules)
{
    public List<string> Validate(LaptopThreshold item)
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

    public void RequireValid(LaptopThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopThreshold> items)
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

/// <summary>Functions of laptop threshold rules.</summary>
public static class LaptopThresholdRules
{
    public static LaptopThresholdValidator DefaultLaptopThresholdValidator()
    {
        return new LaptopThresholdValidator(
            new List<ILaptopThresholdRule>
            {
                new LaptopThresholdLabelLengthRule(32),
                new LaptopThresholdDiscountRangeRule(4.3, 4558.7),
                new LaptopThresholdEmployeeIdLengthRule(16),
                new LaptopThresholdRequiredLabelRule("laptop threshold needs an entry"),
                new LaptopThresholdSequenceRangeRule(64, 86400),
            });
    }
}
