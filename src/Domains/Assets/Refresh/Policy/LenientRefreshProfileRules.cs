namespace Acme.Workforce.Assets.Refresh.Policy;

/// <summary>A rule a lenient refresh profile must pass. Check returns its problems, or none.</summary>
public interface ILenientRefreshProfileRule
{
    List<string> Check(LenientRefreshProfile item);
}

public sealed record LenientRefreshProfileMarginRangeRule(double Min, double Max) : ILenientRefreshProfileRule
{
    public List<string> Check(LenientRefreshProfile item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientRefreshProfileWeightRangeRule(double Min, double Max) : ILenientRefreshProfileRule
{
    public List<string> Check(LenientRefreshProfile item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientRefreshProfileChecksumLengthRule(int MaxLength) : ILenientRefreshProfileRule
{
    public List<string> Check(LenientRefreshProfile item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientRefreshProfileLabelLengthRule(int MaxLength) : ILenientRefreshProfileRule
{
    public List<string> Check(LenientRefreshProfile item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient refresh profile.</summary>
public sealed record LenientRefreshProfileValidator(List<ILenientRefreshProfileRule> Rules)
{
    public List<string> Validate(LenientRefreshProfile item)
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

    public void RequireValid(LenientRefreshProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientRefreshProfile> items)
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

/// <summary>Functions of lenient refresh profile rules.</summary>
public static class LenientRefreshProfileRules
{
    public static LenientRefreshProfileValidator DefaultLenientRefreshProfileValidator()
    {
        return new LenientRefreshProfileValidator(
            new List<ILenientRefreshProfileRule>
            {
                new LenientRefreshProfileMarginRangeRule(2.9, 1274.3),
                new LenientRefreshProfileWeightRangeRule(2.0, 1229.9),
                new LenientRefreshProfileChecksumLengthRule(16),
                new LenientRefreshProfileLabelLengthRule(80),
            });
    }
}
