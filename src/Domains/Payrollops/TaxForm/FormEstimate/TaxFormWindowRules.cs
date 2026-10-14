namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>A rule a tax form window must pass. Check returns its problems, or none.</summary>
public interface ITaxFormWindowRule
{
    List<string> Check(TaxFormWindow item);
}

public sealed record TaxFormWindowCodeLengthRule(int MaxLength) : ITaxFormWindowRule
{
    public List<string> Check(TaxFormWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormWindowSkillLengthRule(int MaxLength) : ITaxFormWindowRule
{
    public List<string> Check(TaxFormWindow item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormWindowNameLengthRule(int MaxLength) : ITaxFormWindowRule
{
    public List<string> Check(TaxFormWindow item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form window.</summary>
public sealed record TaxFormWindowValidator(List<ITaxFormWindowRule> Rules)
{
    public List<string> Validate(TaxFormWindow item)
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

    public void RequireValid(TaxFormWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormWindow> items)
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

/// <summary>Functions of tax form window rules.</summary>
public static class TaxFormWindowRules
{
    public static TaxFormWindowValidator DefaultTaxFormWindowValidator()
    {
        return new TaxFormWindowValidator(
            new List<ITaxFormWindowRule>
            {
                new TaxFormWindowCodeLengthRule(64),
                new TaxFormWindowSkillLengthRule(32),
                new TaxFormWindowNameLengthRule(80),
            });
    }
}
