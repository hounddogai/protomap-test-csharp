namespace Acme.Workforce.Assets.Refresh.RefreshRequest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient refresh request must pass. Check returns its problems, or none.</summary>
public interface ILenientRefreshRequestRule
{
    List<string> Check(LenientRefreshRequest item);
}

public sealed record LenientRefreshRequestAllowedTitleRule(List<string> Allowed) : ILenientRefreshRequestRule
{
    public List<string> Check(LenientRefreshRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record LenientRefreshRequestHoursRangeRule(double Min, double Max) : ILenientRefreshRequestRule
{
    public List<string> Check(LenientRefreshRequest item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientRefreshRequestRequiredCodeRule(string Message) : ILenientRefreshRequestRule
{
    public List<string> Check(LenientRefreshRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientRefreshRequestCodePrefixRule(string Prefix) : ILenientRefreshRequestRule
{
    public List<string> Check(LenientRefreshRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient refresh request.</summary>
public sealed record LenientRefreshRequestValidator(List<ILenientRefreshRequestRule> Rules)
{
    public List<string> Validate(LenientRefreshRequest item)
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

    public void RequireValid(LenientRefreshRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientRefreshRequest> items)
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

/// <summary>Functions of lenient refresh request rules.</summary>
public static class LenientRefreshRequestRules
{
    public static LenientRefreshRequestValidator DefaultLenientRefreshRequestValidator()
    {
        return new LenientRefreshRequestValidator(
            new List<ILenientRefreshRequestRule>
            {
                new LenientRefreshRequestAllowedTitleRule(new List<string> { "seasonal", "legacy", "seasonal" }),
                new LenientRefreshRequestHoursRangeRule(1.6, 2126.9),
                new LenientRefreshRequestRequiredCodeRule("lenient refresh request needs a value"),
                new LenientRefreshRequestCodePrefixRule("Q"),
            });
    }
}
