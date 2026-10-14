namespace Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>A rule a warranty estimate must pass. Check returns its problems, or none.</summary>
public interface IWarrantyEstimateRule
{
    List<string> Check(WarrantyEstimate item);
}

public sealed record WarrantyEstimateLabelLengthRule(int MaxLength) : IWarrantyEstimateRule
{
    public List<string> Check(WarrantyEstimate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyEstimateSequenceRangeRule(int Min, int Max) : IWarrantyEstimateRule
{
    public List<string> Check(WarrantyEstimate item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WarrantyEstimateRetriesRangeRule(int Min, int Max) : IWarrantyEstimateRule
{
    public List<string> Check(WarrantyEstimate item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WarrantyEstimateRegionLengthRule(int MaxLength) : IWarrantyEstimateRule
{
    public List<string> Check(WarrantyEstimate item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyEstimateCodeLengthRule(int MaxLength) : IWarrantyEstimateRule
{
    public List<string> Check(WarrantyEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyEstimateCapacityRangeRule(int Min, int Max) : IWarrantyEstimateRule
{
    public List<string> Check(WarrantyEstimate item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty estimate.</summary>
public sealed record WarrantyEstimateValidator(List<IWarrantyEstimateRule> Rules)
{
    public List<string> Validate(WarrantyEstimate item)
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

    public void RequireValid(WarrantyEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyEstimate> items)
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

/// <summary>Functions of warranty estimate rules.</summary>
public static class WarrantyEstimateRules
{
    public static WarrantyEstimateValidator DefaultWarrantyEstimateValidator()
    {
        return new WarrantyEstimateValidator(
            new List<IWarrantyEstimateRule>
            {
                new WarrantyEstimateLabelLengthRule(120),
                new WarrantyEstimateSequenceRangeRule(255, 1000),
                new WarrantyEstimateRetriesRangeRule(32, 255),
                new WarrantyEstimateRegionLengthRule(120),
                new WarrantyEstimateCodeLengthRule(32),
                new WarrantyEstimateCapacityRangeRule(16, 80),
            });
    }
}
