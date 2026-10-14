namespace Acme.Workforce.Contractors.Vendor.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict vendor request must pass. Check returns its problems, or none.</summary>
public interface IStrictVendorRequestRule
{
    List<string> Check(StrictVendorRequest item);
}

public sealed record StrictVendorRequestAttemptsRangeRule(int Min, int Max) : IStrictVendorRequestRule
{
    public List<string> Check(StrictVendorRequest item)
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

public sealed record StrictVendorRequestMarginRangeRule(double Min, double Max) : IStrictVendorRequestRule
{
    public List<string> Check(StrictVendorRequest item)
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

public sealed record StrictVendorRequestCodePrefixRule(string Prefix) : IStrictVendorRequestRule
{
    public List<string> Check(StrictVendorRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictVendorRequestAllowedExternalRefRule(List<string> Allowed) : IStrictVendorRequestRule
{
    public List<string> Check(StrictVendorRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is stale" };
        }
        return new List<string>();
    }
}

public sealed record StrictVendorRequestRequiredCodeRule(string Message) : IStrictVendorRequestRule
{
    public List<string> Check(StrictVendorRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictVendorRequestRatioRangeRule(double Min, double Max) : IStrictVendorRequestRule
{
    public List<string> Check(StrictVendorRequest item)
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

/// <summary>Runs every rule of a strict vendor request.</summary>
public sealed record StrictVendorRequestValidator(List<IStrictVendorRequestRule> Rules)
{
    public List<string> Validate(StrictVendorRequest item)
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

    public void RequireValid(StrictVendorRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictVendorRequest> items)
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

/// <summary>Functions of strict vendor request rules.</summary>
public static class StrictVendorRequestRules
{
    public static StrictVendorRequestValidator DefaultStrictVendorRequestValidator()
    {
        return new StrictVendorRequestValidator(
            new List<IStrictVendorRequestRule>
            {
                new StrictVendorRequestAttemptsRangeRule(64, 255),
                new StrictVendorRequestMarginRangeRule(7.8, 1235.2),
                new StrictVendorRequestCodePrefixRule("ACM"),
                new StrictVendorRequestAllowedExternalRefRule(new List<string> { "primary", "fallback", "default" }),
                new StrictVendorRequestRequiredCodeRule("strict vendor request needs an entry"),
                new StrictVendorRequestRatioRangeRule(5.3, 1394.4),
            });
    }
}
