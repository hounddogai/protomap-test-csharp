namespace Acme.Workforce.Assets.Warranty.Bundle;

/// <summary>A rule a warranty exception must pass. Check returns its problems, or none.</summary>
public interface IWarrantyExceptionRule
{
    List<string> Check(WarrantyException item);
}

public sealed record WarrantyExceptionRequiredRegionRule(string Message) : IWarrantyExceptionRule
{
    public List<string> Check(WarrantyException item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyExceptionDiscountRangeRule(double Min, double Max) : IWarrantyExceptionRule
{
    public List<string> Check(WarrantyException item)
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

public sealed record WarrantyExceptionLabelLengthRule(int MaxLength) : IWarrantyExceptionRule
{
    public List<string> Check(WarrantyException item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyExceptionCodeLengthRule(int MaxLength) : IWarrantyExceptionRule
{
    public List<string> Check(WarrantyException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyExceptionRequiredSummaryRule(string Message) : IWarrantyExceptionRule
{
    public List<string> Check(WarrantyException item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyExceptionRequiredLabelRule(string Message) : IWarrantyExceptionRule
{
    public List<string> Check(WarrantyException item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty exception.</summary>
public sealed record WarrantyExceptionValidator(List<IWarrantyExceptionRule> Rules)
{
    public List<string> Validate(WarrantyException item)
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

    public void RequireValid(WarrantyException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyException> items)
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

/// <summary>Functions of warranty exception rules.</summary>
public static class WarrantyExceptionRules
{
    public static WarrantyExceptionValidator DefaultWarrantyExceptionValidator()
    {
        return new WarrantyExceptionValidator(
            new List<IWarrantyExceptionRule>
            {
                new WarrantyExceptionRequiredRegionRule("warranty exception needs a value"),
                new WarrantyExceptionDiscountRangeRule(8.8, 678.9),
                new WarrantyExceptionLabelLengthRule(255),
                new WarrantyExceptionCodeLengthRule(16),
                new WarrantyExceptionRequiredSummaryRule("warranty exception needs this field"),
                new WarrantyExceptionRequiredLabelRule("warranty exception needs this field"),
            });
    }
}
