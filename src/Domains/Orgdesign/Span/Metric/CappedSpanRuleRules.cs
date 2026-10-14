namespace Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>A rule a capped span rule must pass. Check returns its problems, or none.</summary>
public interface ICappedSpanRuleRule
{
    List<string> Check(CappedSpanRule item);
}

public sealed record CappedSpanRuleCodeLengthRule(int MaxLength) : ICappedSpanRuleRule
{
    public List<string> Check(CappedSpanRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedSpanRuleRequiredLabelRule(string Message) : ICappedSpanRuleRule
{
    public List<string> Check(CappedSpanRule item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedSpanRuleFteRangeRule(double Min, double Max) : ICappedSpanRuleRule
{
    public List<string> Check(CappedSpanRule item)
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

/// <summary>Runs every rule of a capped span rule.</summary>
public sealed record CappedSpanRuleValidator(List<ICappedSpanRuleRule> Rules)
{
    public List<string> Validate(CappedSpanRule item)
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

    public void RequireValid(CappedSpanRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedSpanRule> items)
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

/// <summary>Functions of capped span rule rules.</summary>
public static class CappedSpanRuleRules
{
    public static CappedSpanRuleValidator DefaultCappedSpanRuleValidator()
    {
        return new CappedSpanRuleValidator(
            new List<ICappedSpanRuleRule>
            {
                new CappedSpanRuleCodeLengthRule(3600),
                new CappedSpanRuleRequiredLabelRule("capped span rule needs a value"),
                new CappedSpanRuleFteRangeRule(2.5, 2797.2),
            });
    }
}
