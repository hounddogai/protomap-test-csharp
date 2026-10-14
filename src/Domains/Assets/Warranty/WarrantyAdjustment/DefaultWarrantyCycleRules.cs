namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default warranty cycle must pass. Check returns its problems, or none.</summary>
public interface IDefaultWarrantyCycleRule
{
    List<string> Check(DefaultWarrantyCycle item);
}

public sealed record DefaultWarrantyCycleRequiredNameRule(string Message) : IDefaultWarrantyCycleRule
{
    public List<string> Check(DefaultWarrantyCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultWarrantyCycleAllowedSummaryRule(List<string> Allowed) : IDefaultWarrantyCycleRule
{
    public List<string> Check(DefaultWarrantyCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is stale" };
        }
        return new List<string>();
    }
}

public sealed record DefaultWarrantyCycleTimezoneLengthRule(int MaxLength) : IDefaultWarrantyCycleRule
{
    public List<string> Check(DefaultWarrantyCycle item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultWarrantyCycleSummaryLengthRule(int MaxLength) : IDefaultWarrantyCycleRule
{
    public List<string> Check(DefaultWarrantyCycle item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default warranty cycle.</summary>
public sealed record DefaultWarrantyCycleValidatorExtra(List<IDefaultWarrantyCycleRule> Rules)
{
    public List<string> Validate(DefaultWarrantyCycle item)
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

    public void RequireValid(DefaultWarrantyCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultWarrantyCycle> items)
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

/// <summary>Functions of default warranty cycle rules.</summary>
public static class DefaultWarrantyCycleRules
{
    public static DefaultWarrantyCycleValidatorExtra DefaultDefaultWarrantyCycleValidator()
    {
        return new DefaultWarrantyCycleValidatorExtra(
            new List<IDefaultWarrantyCycleRule>
            {
                new DefaultWarrantyCycleRequiredNameRule("default warranty cycle needs this field"),
                new DefaultWarrantyCycleAllowedSummaryRule(new List<string> { "seasonal", "rolling", "rolling" }),
                new DefaultWarrantyCycleTimezoneLengthRule(64),
                new DefaultWarrantyCycleSummaryLengthRule(255),
            });
    }
}
