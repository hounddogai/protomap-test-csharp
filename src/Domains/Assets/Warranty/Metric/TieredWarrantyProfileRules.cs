namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered warranty profile must pass. Check returns its problems, or none.</summary>
public interface ITieredWarrantyProfileRule
{
    List<string> Check(TieredWarrantyProfile item);
}

public sealed record TieredWarrantyProfileFteRangeRule(double Min, double Max) : ITieredWarrantyProfileRule
{
    public List<string> Check(TieredWarrantyProfile item)
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

public sealed record TieredWarrantyProfileSegmentLengthRule(int MaxLength) : ITieredWarrantyProfileRule
{
    public List<string> Check(TieredWarrantyProfile item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredWarrantyProfileRequiredCodeRule(string Message) : ITieredWarrantyProfileRule
{
    public List<string> Check(TieredWarrantyProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredWarrantyProfileAllowedSegmentRule(List<string> Allowed) : ITieredWarrantyProfileRule
{
    public List<string> Check(TieredWarrantyProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered warranty profile.</summary>
public sealed record TieredWarrantyProfileValidator(List<ITieredWarrantyProfileRule> Rules)
{
    public List<string> Validate(TieredWarrantyProfile item)
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

    public void RequireValid(TieredWarrantyProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredWarrantyProfile> items)
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

/// <summary>Functions of tiered warranty profile rules.</summary>
public static class TieredWarrantyProfileRules
{
    public static TieredWarrantyProfileValidator DefaultTieredWarrantyProfileValidator()
    {
        return new TieredWarrantyProfileValidator(
            new List<ITieredWarrantyProfileRule>
            {
                new TieredWarrantyProfileFteRangeRule(8.1, 2266.1),
                new TieredWarrantyProfileSegmentLengthRule(120),
                new TieredWarrantyProfileRequiredCodeRule("tiered warranty profile needs this field"),
                new TieredWarrantyProfileAllowedSegmentRule(new List<string>()),
            });
    }
}
