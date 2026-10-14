namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

/// <summary>A rule a tax form plan must pass. Check returns its problems, or none.</summary>
public interface ITaxFormPlanRule
{
    List<string> Check(TaxFormPlan item);
}

public sealed record TaxFormPlanVersionRangeRule(int Min, int Max) : ITaxFormPlanRule
{
    public List<string> Check(TaxFormPlan item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TaxFormPlanCodePrefixRule(string Prefix) : ITaxFormPlanRule
{
    public List<string> Check(TaxFormPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormPlanSalaryBandLengthRule(int MaxLength) : ITaxFormPlanRule
{
    public List<string> Check(TaxFormPlan item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormPlanSkillLengthRule(int MaxLength) : ITaxFormPlanRule
{
    public List<string> Check(TaxFormPlan item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormPlanRequiredTitleRule(string Message) : ITaxFormPlanRule
{
    public List<string> Check(TaxFormPlan item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form plan.</summary>
public sealed record TaxFormPlanValidator(List<ITaxFormPlanRule> Rules)
{
    public List<string> Validate(TaxFormPlan item)
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

    public void RequireValid(TaxFormPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormPlan> items)
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

/// <summary>Functions of tax form plan rules.</summary>
public static class TaxFormPlanRules
{
    public static TaxFormPlanValidator DefaultTaxFormPlanValidator()
    {
        return new TaxFormPlanValidator(
            new List<ITaxFormPlanRule>
            {
                new TaxFormPlanVersionRangeRule(16, 86400),
                new TaxFormPlanCodePrefixRule("Q"),
                new TaxFormPlanSalaryBandLengthRule(3600),
                new TaxFormPlanSkillLengthRule(32),
                new TaxFormPlanRequiredTitleRule("tax form plan needs a value"),
            });
    }
}
