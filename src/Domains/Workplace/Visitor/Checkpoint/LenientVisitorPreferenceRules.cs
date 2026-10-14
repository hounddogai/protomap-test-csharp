namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

/// <summary>A rule a lenient visitor preference must pass. Check returns its problems, or none.</summary>
public interface ILenientVisitorPreferenceRule
{
    List<string> Check(LenientVisitorPreference item);
}

public sealed record LenientVisitorPreferenceCategoryLengthRule(int MaxLength) : ILenientVisitorPreferenceRule
{
    public List<string> Check(LenientVisitorPreference item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorPreferenceRequiredCodeRule(string Message) : ILenientVisitorPreferenceRule
{
    public List<string> Check(LenientVisitorPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorPreferenceCodePrefixRule(string Prefix) : ILenientVisitorPreferenceRule
{
    public List<string> Check(LenientVisitorPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorPreferenceFloorValueRangeRule(double Min, double Max) : ILenientVisitorPreferenceRule
{
    public List<string> Check(LenientVisitorPreference item)
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

public sealed record LenientVisitorPreferenceRequiredLabelRule(string Message) : ILenientVisitorPreferenceRule
{
    public List<string> Check(LenientVisitorPreference item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient visitor preference.</summary>
public sealed record LenientVisitorPreferenceValidator(List<ILenientVisitorPreferenceRule> Rules)
{
    public List<string> Validate(LenientVisitorPreference item)
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

    public void RequireValid(LenientVisitorPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientVisitorPreference> items)
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

/// <summary>Functions of lenient visitor preference rules.</summary>
public static class LenientVisitorPreferenceRules
{
    public static LenientVisitorPreferenceValidator DefaultLenientVisitorPreferenceValidator()
    {
        return new LenientVisitorPreferenceValidator(
            new List<ILenientVisitorPreferenceRule>
            {
                new LenientVisitorPreferenceCategoryLengthRule(16),
                new LenientVisitorPreferenceRequiredCodeRule("lenient visitor preference needs this field"),
                new LenientVisitorPreferenceCodePrefixRule("OPS"),
                new LenientVisitorPreferenceFloorValueRangeRule(3.6, 1185.5),
                new LenientVisitorPreferenceRequiredLabelRule("lenient visitor preference needs a value"),
            });
    }
}
