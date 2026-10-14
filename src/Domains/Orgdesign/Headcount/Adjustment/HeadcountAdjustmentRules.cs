namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>A rule a headcount adjustment must pass. Check returns its problems, or none.</summary>
public interface IHeadcountAdjustmentRule
{
    List<string> Check(HeadcountAdjustment item);
}

public sealed record HeadcountAdjustmentCodePrefixRule(string Prefix) : IHeadcountAdjustmentRule
{
    public List<string> Check(HeadcountAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountAdjustmentRequiredNameRule(string Message) : IHeadcountAdjustmentRule
{
    public List<string> Check(HeadcountAdjustment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountAdjustmentCodeLengthRule(int MaxLength) : IHeadcountAdjustmentRule
{
    public List<string> Check(HeadcountAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountAdjustmentMarginRangeRule(double Min, double Max) : IHeadcountAdjustmentRule
{
    public List<string> Check(HeadcountAdjustment item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a headcount adjustment.</summary>
public sealed record HeadcountAdjustmentValidator(List<IHeadcountAdjustmentRule> Rules)
{
    public List<string> Validate(HeadcountAdjustment item)
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

    public void RequireValid(HeadcountAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountAdjustment> items)
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

/// <summary>Functions of headcount adjustment rules.</summary>
public static class HeadcountAdjustmentRules
{
    public static HeadcountAdjustmentValidator DefaultHeadcountAdjustmentValidator()
    {
        return new HeadcountAdjustmentValidator(
            new List<IHeadcountAdjustmentRule>
            {
                new HeadcountAdjustmentCodePrefixRule("X"),
                new HeadcountAdjustmentRequiredNameRule("headcount adjustment needs this field"),
                new HeadcountAdjustmentCodeLengthRule(255),
                new HeadcountAdjustmentMarginRangeRule(3.2, 2745.4),
            });
    }
}
