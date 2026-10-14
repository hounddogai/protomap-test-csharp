namespace Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>A rule a warranty mapping must pass. Check returns its problems, or none.</summary>
public interface IWarrantyMappingRule
{
    List<string> Check(WarrantyMapping item);
}

public sealed record WarrantyMappingRequiredSegmentRule(string Message) : IWarrantyMappingRule
{
    public List<string> Check(WarrantyMapping item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyMappingSegmentLengthRule(int MaxLength) : IWarrantyMappingRule
{
    public List<string> Check(WarrantyMapping item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyMappingRequiredCodeRule(string Message) : IWarrantyMappingRule
{
    public List<string> Check(WarrantyMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty mapping.</summary>
public sealed record WarrantyMappingValidator(List<IWarrantyMappingRule> Rules)
{
    public List<string> Validate(WarrantyMapping item)
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

    public void RequireValid(WarrantyMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyMapping> items)
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

/// <summary>Functions of warranty mapping rules.</summary>
public static class WarrantyMappingRules
{
    public static WarrantyMappingValidator DefaultWarrantyMappingValidator()
    {
        return new WarrantyMappingValidator(
            new List<IWarrantyMappingRule>
            {
                new WarrantyMappingRequiredSegmentRule("warranty mapping needs an entry"),
                new WarrantyMappingSegmentLengthRule(16),
                new WarrantyMappingRequiredCodeRule("warranty mapping needs an entry"),
            });
    }
}
