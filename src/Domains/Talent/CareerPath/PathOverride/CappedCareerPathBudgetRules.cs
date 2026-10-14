namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped career path budget must pass. Check returns its problems, or none.</summary>
public interface ICappedCareerPathBudgetRule
{
    List<string> Check(CappedCareerPathBudget item);
}

public sealed record CappedCareerPathBudgetTenureMonthsRangeRule(int Min, int Max) : ICappedCareerPathBudgetRule
{
    public List<string> Check(CappedCareerPathBudget item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedCareerPathBudgetAllowedLabelRule(List<string> Allowed) : ICappedCareerPathBudgetRule
{
    public List<string> Check(CappedCareerPathBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is expired" };
        }
        return new List<string>();
    }
}

public sealed record CappedCareerPathBudgetQuantityRangeRule(int Min, int Max) : ICappedCareerPathBudgetRule
{
    public List<string> Check(CappedCareerPathBudget item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedCareerPathBudgetCodePrefixRule(string Prefix) : ICappedCareerPathBudgetRule
{
    public List<string> Check(CappedCareerPathBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped career path budget.</summary>
public sealed record CappedCareerPathBudgetValidator(List<ICappedCareerPathBudgetRule> Rules)
{
    public List<string> Validate(CappedCareerPathBudget item)
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

    public void RequireValid(CappedCareerPathBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedCareerPathBudget> items)
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

/// <summary>Functions of capped career path budget rules.</summary>
public static class CappedCareerPathBudgetRules
{
    public static CappedCareerPathBudgetValidator DefaultCappedCareerPathBudgetValidator()
    {
        return new CappedCareerPathBudgetValidator(
            new List<ICappedCareerPathBudgetRule>
            {
                new CappedCareerPathBudgetTenureMonthsRangeRule(32, 80),
                new CappedCareerPathBudgetAllowedLabelRule(new List<string> { "primary" }),
                new CappedCareerPathBudgetQuantityRangeRule(120, 3600),
                new CappedCareerPathBudgetCodePrefixRule("OPS"),
            });
    }
}
