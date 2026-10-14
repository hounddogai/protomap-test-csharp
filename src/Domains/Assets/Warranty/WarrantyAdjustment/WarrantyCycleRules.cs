namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a warranty cycle must pass. Check returns its problems, or none.</summary>
public interface IWarrantyCycleRule
{
    List<string> Check(WarrantyCycle item);
}

public sealed record WarrantyCycleCodeLengthRule(int MaxLength) : IWarrantyCycleRule
{
    public List<string> Check(WarrantyCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyCycleRequiredCodeRule(string Message) : IWarrantyCycleRule
{
    public List<string> Check(WarrantyCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyCycleRequiredLabelRule(string Message) : IWarrantyCycleRule
{
    public List<string> Check(WarrantyCycle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyCycleAllowedCategoryRule(List<string> Allowed) : IWarrantyCycleRule
{
    public List<string> Check(WarrantyCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyCycleRevisionRangeRule(int Min, int Max) : IWarrantyCycleRule
{
    public List<string> Check(WarrantyCycle item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a warranty cycle.</summary>
public sealed record WarrantyCycleValidator(List<IWarrantyCycleRule> Rules)
{
    public List<string> Validate(WarrantyCycle item)
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

    public void RequireValid(WarrantyCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyCycle> items)
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

/// <summary>Functions of warranty cycle rules.</summary>
public static class WarrantyCycleRules
{
    public static WarrantyCycleValidator DefaultWarrantyCycleValidator()
    {
        return new WarrantyCycleValidator(
            new List<IWarrantyCycleRule>
            {
                new WarrantyCycleCodeLengthRule(1000),
                new WarrantyCycleRequiredCodeRule("warranty cycle needs an entry"),
                new WarrantyCycleRequiredLabelRule("warranty cycle needs this field"),
                new WarrantyCycleAllowedCategoryRule(new List<string>()),
                new WarrantyCycleRevisionRangeRule(16, 64),
            });
    }
}
