namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient mobility request must pass. Check returns its problems, or none.</summary>
public interface ILenientMobilityRequestRule
{
    List<string> Check(LenientMobilityRequest item);
}

public sealed record LenientMobilityRequestCodePrefixRule(string Prefix) : ILenientMobilityRequestRule
{
    public List<string> Check(LenientMobilityRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientMobilityRequestAllowedCodeRule(List<string> Allowed) : ILenientMobilityRequestRule
{
    public List<string> Check(LenientMobilityRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record LenientMobilityRequestRequiredCodeRule(string Message) : ILenientMobilityRequestRule
{
    public List<string> Check(LenientMobilityRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientMobilityRequestRatioRangeRule(double Min, double Max) : ILenientMobilityRequestRule
{
    public List<string> Check(LenientMobilityRequest item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientMobilityRequestRequiredCostCenterRule(string Message) : ILenientMobilityRequestRule
{
    public List<string> Check(LenientMobilityRequest item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient mobility request.</summary>
public sealed record LenientMobilityRequestValidator(List<ILenientMobilityRequestRule> Rules)
{
    public List<string> Validate(LenientMobilityRequest item)
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

    public void RequireValid(LenientMobilityRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientMobilityRequest> items)
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

/// <summary>Functions of lenient mobility request rules.</summary>
public static class LenientMobilityRequestRules
{
    public static LenientMobilityRequestValidator DefaultLenientMobilityRequestValidator()
    {
        return new LenientMobilityRequestValidator(
            new List<ILenientMobilityRequestRule>
            {
                new LenientMobilityRequestCodePrefixRule("OPS"),
                new LenientMobilityRequestAllowedCodeRule(new List<string> { "lenient", "default", "tiered" }),
                new LenientMobilityRequestRequiredCodeRule("lenient mobility request needs a value"),
                new LenientMobilityRequestRatioRangeRule(8.9, 4093.2),
                new LenientMobilityRequestRequiredCostCenterRule("lenient mobility request needs this field"),
            });
    }
}
