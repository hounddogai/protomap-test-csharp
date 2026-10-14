namespace Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>A rule a legacy vendor batch must pass. Check returns its problems, or none.</summary>
public interface ILegacyVendorBatchRule
{
    List<string> Check(LegacyVendorBatch item);
}

public sealed record LegacyVendorBatchCodeLengthRule(int MaxLength) : ILegacyVendorBatchRule
{
    public List<string> Check(LegacyVendorBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyVendorBatchRatingRangeRule(double Min, double Max) : ILegacyVendorBatchRule
{
    public List<string> Check(LegacyVendorBatch item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyVendorBatchRequiredLabelRule(string Message) : ILegacyVendorBatchRule
{
    public List<string> Check(LegacyVendorBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyVendorBatchRequiredTitleRule(string Message) : ILegacyVendorBatchRule
{
    public List<string> Check(LegacyVendorBatch item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy vendor batch.</summary>
public sealed record LegacyVendorBatchValidator(List<ILegacyVendorBatchRule> Rules)
{
    public List<string> Validate(LegacyVendorBatch item)
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

    public void RequireValid(LegacyVendorBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyVendorBatch> items)
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

/// <summary>Functions of legacy vendor batch rules.</summary>
public static class LegacyVendorBatchRules
{
    public static LegacyVendorBatchValidator DefaultLegacyVendorBatchValidator()
    {
        return new LegacyVendorBatchValidator(
            new List<ILegacyVendorBatchRule>
            {
                new LegacyVendorBatchCodeLengthRule(80),
                new LegacyVendorBatchRatingRangeRule(4.7, 303.5),
                new LegacyVendorBatchRequiredLabelRule("legacy vendor batch needs an entry"),
                new LegacyVendorBatchRequiredTitleRule("legacy vendor batch needs an entry"),
            });
    }
}
