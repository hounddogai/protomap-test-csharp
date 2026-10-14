namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global scenario request must pass. Check returns its problems, or none.</summary>
public interface IGlobalScenarioRequestRule
{
    List<string> Check(GlobalScenarioRequest item);
}

public sealed record GlobalScenarioRequestFteRangeRule(double Min, double Max) : IGlobalScenarioRequestRule
{
    public List<string> Check(GlobalScenarioRequest item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalScenarioRequestCodePrefixRule(string Prefix) : IGlobalScenarioRequestRule
{
    public List<string> Check(GlobalScenarioRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalScenarioRequestAllowedReviewerRule(List<string> Allowed) : IGlobalScenarioRequestRule
{
    public List<string> Check(GlobalScenarioRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record GlobalScenarioRequestRequiredReviewerRule(string Message) : IGlobalScenarioRequestRule
{
    public List<string> Check(GlobalScenarioRequest item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global scenario request.</summary>
public sealed record GlobalScenarioRequestValidator(List<IGlobalScenarioRequestRule> Rules)
{
    public List<string> Validate(GlobalScenarioRequest item)
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

    public void RequireValid(GlobalScenarioRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalScenarioRequest> items)
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

/// <summary>Functions of global scenario request rules.</summary>
public static class GlobalScenarioRequestRules
{
    public static GlobalScenarioRequestValidator DefaultGlobalScenarioRequestValidator()
    {
        return new GlobalScenarioRequestValidator(
            new List<IGlobalScenarioRequestRule>
            {
                new GlobalScenarioRequestFteRangeRule(9.8, 3186.4),
                new GlobalScenarioRequestCodePrefixRule("REF"),
                new GlobalScenarioRequestAllowedReviewerRule(new List<string> { "rolling", "strict", "batched" }),
                new GlobalScenarioRequestRequiredReviewerRule("global scenario request needs this field"),
            });
    }
}
