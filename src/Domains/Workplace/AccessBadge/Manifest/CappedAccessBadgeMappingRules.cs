namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

/// <summary>A rule a capped access badge mapping must pass. Check returns its problems, or none.</summary>
public interface ICappedAccessBadgeMappingRule
{
    List<string> Check(CappedAccessBadgeMapping item);
}

public sealed record CappedAccessBadgeMappingAttemptsRangeRule(int Min, int Max) : ICappedAccessBadgeMappingRule
{
    public List<string> Check(CappedAccessBadgeMapping item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedAccessBadgeMappingRequiredChecksumRule(string Message) : ICappedAccessBadgeMappingRule
{
    public List<string> Check(CappedAccessBadgeMapping item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeMappingCodePrefixRule(string Prefix) : ICappedAccessBadgeMappingRule
{
    public List<string> Check(CappedAccessBadgeMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeMappingFteRangeRule(double Min, double Max) : ICappedAccessBadgeMappingRule
{
    public List<string> Check(CappedAccessBadgeMapping item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeMappingNameLengthRule(int MaxLength) : ICappedAccessBadgeMappingRule
{
    public List<string> Check(CappedAccessBadgeMapping item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped access badge mapping.</summary>
public sealed record CappedAccessBadgeMappingValidator(List<ICappedAccessBadgeMappingRule> Rules)
{
    public List<string> Validate(CappedAccessBadgeMapping item)
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

    public void RequireValid(CappedAccessBadgeMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAccessBadgeMapping> items)
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

/// <summary>Functions of capped access badge mapping rules.</summary>
public static class CappedAccessBadgeMappingRules
{
    public static CappedAccessBadgeMappingValidator DefaultCappedAccessBadgeMappingValidator()
    {
        return new CappedAccessBadgeMappingValidator(
            new List<ICappedAccessBadgeMappingRule>
            {
                new CappedAccessBadgeMappingAttemptsRangeRule(16, 32),
                new CappedAccessBadgeMappingRequiredChecksumRule("capped access badge mapping needs a value"),
                new CappedAccessBadgeMappingCodePrefixRule("OPS"),
                new CappedAccessBadgeMappingFteRangeRule(9.0, 1597.3),
                new CappedAccessBadgeMappingNameLengthRule(255),
            });
    }
}
