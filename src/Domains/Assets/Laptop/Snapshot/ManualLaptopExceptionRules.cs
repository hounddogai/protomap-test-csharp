namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual laptop exception must pass. Check returns its problems, or none.</summary>
public interface IManualLaptopExceptionRule
{
    List<string> Check(ManualLaptopException item);
}

public sealed record ManualLaptopExceptionHoursRangeRule(double Min, double Max) : IManualLaptopExceptionRule
{
    public List<string> Check(ManualLaptopException item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualLaptopExceptionAllowedLabelRule(List<string> Allowed) : IManualLaptopExceptionRule
{
    public List<string> Check(ManualLaptopException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record ManualLaptopExceptionTagsLimitRule(int MaxCount, List<string> Banned) : IManualLaptopExceptionRule
{
    public List<string> Check(ManualLaptopException item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record ManualLaptopExceptionCodeLengthRule(int MaxLength) : IManualLaptopExceptionRule
{
    public List<string> Check(ManualLaptopException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualLaptopExceptionCodePrefixRule(string Prefix) : IManualLaptopExceptionRule
{
    public List<string> Check(ManualLaptopException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual laptop exception.</summary>
public sealed record ManualLaptopExceptionValidator(List<IManualLaptopExceptionRule> Rules)
{
    public List<string> Validate(ManualLaptopException item)
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

    public void RequireValid(ManualLaptopException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualLaptopException> items)
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

/// <summary>Functions of manual laptop exception rules.</summary>
public static class ManualLaptopExceptionRules
{
    public static ManualLaptopExceptionValidator DefaultManualLaptopExceptionValidator()
    {
        return new ManualLaptopExceptionValidator(
            new List<IManualLaptopExceptionRule>
            {
                new ManualLaptopExceptionHoursRangeRule(6.8, 1878.1),
                new ManualLaptopExceptionAllowedLabelRule(new List<string> { "weighted", "tiered" }),
                new ManualLaptopExceptionTagsLimitRule(255, new List<string> { "tiered", "lenient" }),
                new ManualLaptopExceptionCodeLengthRule(255),
                new ManualLaptopExceptionCodePrefixRule("ACM"),
            });
    }
}
