namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient deduction profile must pass. Check returns its problems, or none.</summary>
public interface ILenientDeductionProfileRule
{
    List<string> Check(LenientDeductionProfile item);
}

public sealed record LenientDeductionProfileRequiredNameRule(string Message) : ILenientDeductionProfileRule
{
    public List<string> Check(LenientDeductionProfile item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionProfileAllowedSummaryRule(List<string> Allowed) : ILenientDeductionProfileRule
{
    public List<string> Check(LenientDeductionProfile item)
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

public sealed record LenientDeductionProfileCeilingRangeRule(double Min, double Max) : ILenientDeductionProfileRule
{
    public List<string> Check(LenientDeductionProfile item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionProfileRatioRangeRule(double Min, double Max) : ILenientDeductionProfileRule
{
    public List<string> Check(LenientDeductionProfile item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionProfileRequiredCategoryRule(string Message) : ILenientDeductionProfileRule
{
    public List<string> Check(LenientDeductionProfile item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient deduction profile.</summary>
public sealed record LenientDeductionProfileValidator(List<ILenientDeductionProfileRule> Rules)
{
    public List<string> Validate(LenientDeductionProfile item)
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

    public void RequireValid(LenientDeductionProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientDeductionProfile> items)
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

/// <summary>Functions of lenient deduction profile rules.</summary>
public static class LenientDeductionProfileRules
{
    public static LenientDeductionProfileValidator DefaultLenientDeductionProfileValidator()
    {
        return new LenientDeductionProfileValidator(
            new List<ILenientDeductionProfileRule>
            {
                new LenientDeductionProfileRequiredNameRule("lenient deduction profile needs a value"),
                new LenientDeductionProfileAllowedSummaryRule(new List<string> { "lenient", "capped" }),
                new LenientDeductionProfileCeilingRangeRule(2.6, 4763.1),
                new LenientDeductionProfileRatioRangeRule(1.8, 1049.2),
                new LenientDeductionProfileRequiredCategoryRule("lenient deduction profile needs a value"),
            });
    }
}
