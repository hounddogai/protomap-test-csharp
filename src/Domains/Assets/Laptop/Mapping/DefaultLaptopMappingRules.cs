namespace Acme.Workforce.Assets.Laptop.Mapping;

/// <summary>A rule a default laptop mapping must pass. Check returns its problems, or none.</summary>
public interface IDefaultLaptopMappingRule
{
    List<string> Check(DefaultLaptopMapping item);
}

public sealed record DefaultLaptopMappingRequiredNameRule(string Message) : IDefaultLaptopMappingRule
{
    public List<string> Check(DefaultLaptopMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultLaptopMappingCodeLengthRule(int MaxLength) : IDefaultLaptopMappingRule
{
    public List<string> Check(DefaultLaptopMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultLaptopMappingCodePrefixRule(string Prefix) : IDefaultLaptopMappingRule
{
    public List<string> Check(DefaultLaptopMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultLaptopMappingTenureMonthsRangeRule(int Min, int Max) : IDefaultLaptopMappingRule
{
    public List<string> Check(DefaultLaptopMapping item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultLaptopMappingRequiredCodeRule(string Message) : IDefaultLaptopMappingRule
{
    public List<string> Check(DefaultLaptopMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultLaptopMappingNotesLengthRule(int MaxLength) : IDefaultLaptopMappingRule
{
    public List<string> Check(DefaultLaptopMapping item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default laptop mapping.</summary>
public sealed record DefaultLaptopMappingValidator(List<IDefaultLaptopMappingRule> Rules)
{
    public List<string> Validate(DefaultLaptopMapping item)
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

    public void RequireValid(DefaultLaptopMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultLaptopMapping> items)
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

/// <summary>Functions of default laptop mapping rules.</summary>
public static class DefaultLaptopMappingRules
{
    public static DefaultLaptopMappingValidator DefaultDefaultLaptopMappingValidator()
    {
        return new DefaultLaptopMappingValidator(
            new List<IDefaultLaptopMappingRule>
            {
                new DefaultLaptopMappingRequiredNameRule("default laptop mapping needs an entry"),
                new DefaultLaptopMappingCodeLengthRule(1000),
                new DefaultLaptopMappingCodePrefixRule("ACM"),
                new DefaultLaptopMappingTenureMonthsRangeRule(80, 1000),
                new DefaultLaptopMappingRequiredCodeRule("default laptop mapping needs an entry"),
                new DefaultLaptopMappingNotesLengthRule(3600),
            });
    }
}
