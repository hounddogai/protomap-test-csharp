namespace Acme.Workforce.Assets.Warranty.Summary;

/// <summary>A rule a warranty override must pass. Check returns its problems, or none.</summary>
public interface IWarrantyOverrideRule
{
    List<string> Check(WarrantyOverride item);
}

public sealed record WarrantyOverrideStartsAtRangeRule(int Min, int Max) : IWarrantyOverrideRule
{
    public List<string> Check(WarrantyOverride item)
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

public sealed record WarrantyOverrideCodeLengthRule(int MaxLength) : IWarrantyOverrideRule
{
    public List<string> Check(WarrantyOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyOverrideRequiredTitleRule(string Message) : IWarrantyOverrideRule
{
    public List<string> Check(WarrantyOverride item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty override.</summary>
public sealed record WarrantyOverrideValidator(List<IWarrantyOverrideRule> Rules)
{
    public List<string> Validate(WarrantyOverride item)
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

    public void RequireValid(WarrantyOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyOverride> items)
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

/// <summary>Functions of warranty override rules.</summary>
public static class WarrantyOverrideRules
{
    public static WarrantyOverrideValidator DefaultWarrantyOverrideValidator()
    {
        return new WarrantyOverrideValidator(
            new List<IWarrantyOverrideRule>
            {
                new WarrantyOverrideStartsAtRangeRule(32, 86400),
                new WarrantyOverrideCodeLengthRule(80),
                new WarrantyOverrideRequiredTitleRule("warranty override needs a value"),
            });
    }
}
