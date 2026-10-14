namespace Acme.Workforce.Payrollops.TaxForm.FormTemplate;

/// <summary>A rule a primary tax form template must pass. Check returns its problems, or none.</summary>
public interface IPrimaryTaxFormTemplateRule
{
    List<string> Check(PrimaryTaxFormTemplate item);
}

public sealed record PrimaryTaxFormTemplateRequiredNameRule(string Message) : IPrimaryTaxFormTemplateRule
{
    public List<string> Check(PrimaryTaxFormTemplate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTaxFormTemplateMaximumRangeRule(double Min, double Max) : IPrimaryTaxFormTemplateRule
{
    public List<string> Check(PrimaryTaxFormTemplate item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryTaxFormTemplateNameLengthRule(int MaxLength) : IPrimaryTaxFormTemplateRule
{
    public List<string> Check(PrimaryTaxFormTemplate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTaxFormTemplateEmployeeIdLengthRule(int MaxLength) : IPrimaryTaxFormTemplateRule
{
    public List<string> Check(PrimaryTaxFormTemplate item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTaxFormTemplateRequiredEmployeeIdRule(string Message) : IPrimaryTaxFormTemplateRule
{
    public List<string> Check(PrimaryTaxFormTemplate item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary tax form template.</summary>
public sealed record PrimaryTaxFormTemplateValidator(List<IPrimaryTaxFormTemplateRule> Rules)
{
    public List<string> Validate(PrimaryTaxFormTemplate item)
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

    public void RequireValid(PrimaryTaxFormTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryTaxFormTemplate> items)
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

/// <summary>Functions of primary tax form template rules.</summary>
public static class PrimaryTaxFormTemplateRules
{
    public static PrimaryTaxFormTemplateValidator DefaultPrimaryTaxFormTemplateValidator()
    {
        return new PrimaryTaxFormTemplateValidator(
            new List<IPrimaryTaxFormTemplateRule>
            {
                new PrimaryTaxFormTemplateRequiredNameRule("primary tax form template needs a value"),
                new PrimaryTaxFormTemplateMaximumRangeRule(3.3, 1969.9),
                new PrimaryTaxFormTemplateNameLengthRule(120),
                new PrimaryTaxFormTemplateEmployeeIdLengthRule(64),
                new PrimaryTaxFormTemplateRequiredEmployeeIdRule("primary tax form template needs an entry"),
            });
    }
}
