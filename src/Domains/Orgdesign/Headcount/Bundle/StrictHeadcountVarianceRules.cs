namespace Acme.Workforce.Orgdesign.Headcount.Bundle;

/// <summary>A rule a strict headcount variance must pass. Check returns its problems, or none.</summary>
public interface IStrictHeadcountVarianceRule
{
    List<string> Check(StrictHeadcountVariance item);
}

public sealed record StrictHeadcountVarianceRequiredUnitRule(string Message) : IStrictHeadcountVarianceRule
{
    public List<string> Check(StrictHeadcountVariance item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictHeadcountVarianceRequiredCodeRule(string Message) : IStrictHeadcountVarianceRule
{
    public List<string> Check(StrictHeadcountVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictHeadcountVarianceCodeLengthRule(int MaxLength) : IStrictHeadcountVarianceRule
{
    public List<string> Check(StrictHeadcountVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictHeadcountVarianceRequiredCostCenterRule(string Message) : IStrictHeadcountVarianceRule
{
    public List<string> Check(StrictHeadcountVariance item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict headcount variance.</summary>
public sealed record StrictHeadcountVarianceValidator(List<IStrictHeadcountVarianceRule> Rules)
{
    public List<string> Validate(StrictHeadcountVariance item)
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

    public void RequireValid(StrictHeadcountVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictHeadcountVariance> items)
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

/// <summary>Functions of strict headcount variance rules.</summary>
public static class StrictHeadcountVarianceRules
{
    public static StrictHeadcountVarianceValidator DefaultStrictHeadcountVarianceValidator()
    {
        return new StrictHeadcountVarianceValidator(
            new List<IStrictHeadcountVarianceRule>
            {
                new StrictHeadcountVarianceRequiredUnitRule("strict headcount variance needs a value"),
                new StrictHeadcountVarianceRequiredCodeRule("strict headcount variance needs an entry"),
                new StrictHeadcountVarianceCodeLengthRule(16),
                new StrictHeadcountVarianceRequiredCostCenterRule("strict headcount variance needs this field"),
            });
    }
}
