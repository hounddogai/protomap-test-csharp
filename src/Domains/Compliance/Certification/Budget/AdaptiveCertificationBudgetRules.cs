namespace Acme.Workforce.Compliance.Certification.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive certification budget must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveCertificationBudgetRule
{
    List<string> Check(AdaptiveCertificationBudget item);
}

public sealed record AdaptiveCertificationBudgetMarginRangeRule(double Min, double Max) : IAdaptiveCertificationBudgetRule
{
    public List<string> Check(AdaptiveCertificationBudget item)
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

public sealed record AdaptiveCertificationBudgetRequiredCodeRule(string Message) : IAdaptiveCertificationBudgetRule
{
    public List<string> Check(AdaptiveCertificationBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCertificationBudgetWindowDaysRangeRule(int Min, int Max) : IAdaptiveCertificationBudgetRule
{
    public List<string> Check(AdaptiveCertificationBudget item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveCertificationBudgetAllowedCodeRule(List<string> Allowed) : IAdaptiveCertificationBudgetRule
{
    public List<string> Check(AdaptiveCertificationBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCertificationBudgetFteRangeRule(double Min, double Max) : IAdaptiveCertificationBudgetRule
{
    public List<string> Check(AdaptiveCertificationBudget item)
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

public sealed record AdaptiveCertificationBudgetCodePrefixRule(string Prefix) : IAdaptiveCertificationBudgetRule
{
    public List<string> Check(AdaptiveCertificationBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive certification budget.</summary>
public sealed record AdaptiveCertificationBudgetValidator(List<IAdaptiveCertificationBudgetRule> Rules)
{
    public List<string> Validate(AdaptiveCertificationBudget item)
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

    public void RequireValid(AdaptiveCertificationBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveCertificationBudget> items)
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

/// <summary>Functions of adaptive certification budget rules.</summary>
public static class AdaptiveCertificationBudgetRules
{
    public static AdaptiveCertificationBudgetValidator DefaultAdaptiveCertificationBudgetValidator()
    {
        return new AdaptiveCertificationBudgetValidator(
            new List<IAdaptiveCertificationBudgetRule>
            {
                new AdaptiveCertificationBudgetMarginRangeRule(7.9, 1597.9),
                new AdaptiveCertificationBudgetRequiredCodeRule("adaptive certification budget needs this field"),
                new AdaptiveCertificationBudgetWindowDaysRangeRule(32, 1000),
                new AdaptiveCertificationBudgetAllowedCodeRule(
                    new List<string> { "rolling", "weighted", "fallback" }),
                new AdaptiveCertificationBudgetFteRangeRule(3.0, 395.6),
                new AdaptiveCertificationBudgetCodePrefixRule("Q"),
            });
    }
}
