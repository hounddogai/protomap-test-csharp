namespace Acme.Workforce.Orgdesign.Headcount.Entry;

/// <summary>A rule a default headcount entry must pass. Check returns its problems, or none.</summary>
public interface IDefaultHeadcountEntryRule
{
    List<string> Check(DefaultHeadcountEntry item);
}

public sealed record DefaultHeadcountEntryCodeLengthRule(int MaxLength) : IDefaultHeadcountEntryRule
{
    public List<string> Check(DefaultHeadcountEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHeadcountEntryCodePrefixRule(string Prefix) : IDefaultHeadcountEntryRule
{
    public List<string> Check(DefaultHeadcountEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHeadcountEntryRequiredLabelRule(string Message) : IDefaultHeadcountEntryRule
{
    public List<string> Check(DefaultHeadcountEntry item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultHeadcountEntryRequiredChecksumRule(string Message) : IDefaultHeadcountEntryRule
{
    public List<string> Check(DefaultHeadcountEntry item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultHeadcountEntryFloorValueRangeRule(double Min, double Max) : IDefaultHeadcountEntryRule
{
    public List<string> Check(DefaultHeadcountEntry item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultHeadcountEntryWeightRangeRule(double Min, double Max) : IDefaultHeadcountEntryRule
{
    public List<string> Check(DefaultHeadcountEntry item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a default headcount entry.</summary>
public sealed record DefaultHeadcountEntryValidatorExtra(List<IDefaultHeadcountEntryRule> Rules)
{
    public List<string> Validate(DefaultHeadcountEntry item)
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

    public void RequireValid(DefaultHeadcountEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultHeadcountEntry> items)
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

/// <summary>Functions of default headcount entry rules.</summary>
public static class DefaultHeadcountEntryRules
{
    public static DefaultHeadcountEntryValidatorExtra DefaultDefaultHeadcountEntryValidator()
    {
        return new DefaultHeadcountEntryValidatorExtra(
            new List<IDefaultHeadcountEntryRule>
            {
                new DefaultHeadcountEntryCodeLengthRule(255),
                new DefaultHeadcountEntryCodePrefixRule("ACM"),
                new DefaultHeadcountEntryRequiredLabelRule("default headcount entry needs this field"),
                new DefaultHeadcountEntryRequiredChecksumRule("default headcount entry needs a value"),
                new DefaultHeadcountEntryFloorValueRangeRule(6.2, 715.8),
                new DefaultHeadcountEntryWeightRangeRule(7.7, 1120.9),
            });
    }
}
