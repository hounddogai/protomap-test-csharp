namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered warranty override must pass. Check returns its problems, or none.</summary>
public interface ITieredWarrantyOverrideRule
{
    List<string> Check(TieredWarrantyOverride item);
}

public sealed record TieredWarrantyOverrideLeaveDaysRangeRule(int Min, int Max) : ITieredWarrantyOverrideRule
{
    public List<string> Check(TieredWarrantyOverride item)
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

public sealed record TieredWarrantyOverrideAllowedManagerIdRule(List<string> Allowed) : ITieredWarrantyOverrideRule
{
    public List<string> Check(TieredWarrantyOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record TieredWarrantyOverrideTitleLengthRule(int MaxLength) : ITieredWarrantyOverrideRule
{
    public List<string> Check(TieredWarrantyOverride item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredWarrantyOverrideCodeLengthRule(int MaxLength) : ITieredWarrantyOverrideRule
{
    public List<string> Check(TieredWarrantyOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredWarrantyOverrideRateRangeRule(double Min, double Max) : ITieredWarrantyOverrideRule
{
    public List<string> Check(TieredWarrantyOverride item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredWarrantyOverrideSourceLengthRule(int MaxLength) : ITieredWarrantyOverrideRule
{
    public List<string> Check(TieredWarrantyOverride item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered warranty override.</summary>
public sealed record TieredWarrantyOverrideValidator(List<ITieredWarrantyOverrideRule> Rules)
{
    public List<string> Validate(TieredWarrantyOverride item)
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

    public void RequireValid(TieredWarrantyOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredWarrantyOverride> items)
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

/// <summary>Functions of tiered warranty override rules.</summary>
public static class TieredWarrantyOverrideRules
{
    public static TieredWarrantyOverrideValidator DefaultTieredWarrantyOverrideValidator()
    {
        return new TieredWarrantyOverrideValidator(
            new List<ITieredWarrantyOverrideRule>
            {
                new TieredWarrantyOverrideLeaveDaysRangeRule(64, 255),
                new TieredWarrantyOverrideAllowedManagerIdRule(new List<string>()),
                new TieredWarrantyOverrideTitleLengthRule(16),
                new TieredWarrantyOverrideCodeLengthRule(64),
                new TieredWarrantyOverrideRateRangeRule(3.4, 3669.3),
                new TieredWarrantyOverrideSourceLengthRule(86400),
            });
    }
}
