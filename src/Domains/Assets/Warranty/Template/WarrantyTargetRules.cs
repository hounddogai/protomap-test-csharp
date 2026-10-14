namespace Acme.Workforce.Assets.Warranty.Template;

/// <summary>A rule a warranty target must pass. Check returns its problems, or none.</summary>
public interface IWarrantyTargetRule
{
    List<string> Check(WarrantyTarget item);
}

public sealed record WarrantyTargetCodePrefixRule(string Prefix) : IWarrantyTargetRule
{
    public List<string> Check(WarrantyTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyTargetQuantityRangeRule(int Min, int Max) : IWarrantyTargetRule
{
    public List<string> Check(WarrantyTarget item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyTargetCategoryLengthRule(int MaxLength) : IWarrantyTargetRule
{
    public List<string> Check(WarrantyTarget item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyTargetLabelLengthRule(int MaxLength) : IWarrantyTargetRule
{
    public List<string> Check(WarrantyTarget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyTargetLeaveDaysRangeRule(int Min, int Max) : IWarrantyTargetRule
{
    public List<string> Check(WarrantyTarget item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a warranty target.</summary>
public sealed record WarrantyTargetValidator(List<IWarrantyTargetRule> Rules)
{
    public List<string> Validate(WarrantyTarget item)
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

    public void RequireValid(WarrantyTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyTarget> items)
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

/// <summary>Functions of warranty target rules.</summary>
public static class WarrantyTargetRules
{
    public static WarrantyTargetValidator DefaultWarrantyTargetValidator()
    {
        return new WarrantyTargetValidator(
            new List<IWarrantyTargetRule>
            {
                new WarrantyTargetCodePrefixRule("Q"),
                new WarrantyTargetQuantityRangeRule(1000, 3600),
                new WarrantyTargetCategoryLengthRule(16),
                new WarrantyTargetLabelLengthRule(16),
                new WarrantyTargetLeaveDaysRangeRule(255, 1000),
            });
    }
}
