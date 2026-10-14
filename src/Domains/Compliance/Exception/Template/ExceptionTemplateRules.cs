namespace Acme.Workforce.Compliance.Exception.Template;

/// <summary>A rule a exception template must pass. Check returns its problems, or none.</summary>
public interface IExceptionTemplateRule
{
    List<string> Check(ExceptionTemplate item);
}

public sealed record ExceptionTemplateRequiredCodeRule(string Message) : IExceptionTemplateRule
{
    public List<string> Check(ExceptionTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionTemplateDescriptionLengthRule(int MaxLength) : IExceptionTemplateRule
{
    public List<string> Check(ExceptionTemplate item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionTemplateRequiredDescriptionRule(string Message) : IExceptionTemplateRule
{
    public List<string> Check(ExceptionTemplate item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionTemplateCurrencyLengthRule(int MaxLength) : IExceptionTemplateRule
{
    public List<string> Check(ExceptionTemplate item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionTemplateTitleLengthRule(int MaxLength) : IExceptionTemplateRule
{
    public List<string> Check(ExceptionTemplate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception template.</summary>
public sealed record ExceptionTemplateValidator(List<IExceptionTemplateRule> Rules)
{
    public List<string> Validate(ExceptionTemplate item)
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

    public void RequireValid(ExceptionTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionTemplate> items)
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

/// <summary>Functions of exception template rules.</summary>
public static class ExceptionTemplateRules
{
    public static ExceptionTemplateValidator DefaultExceptionTemplateValidator()
    {
        return new ExceptionTemplateValidator(
            new List<IExceptionTemplateRule>
            {
                new ExceptionTemplateRequiredCodeRule("exception template needs this field"),
                new ExceptionTemplateDescriptionLengthRule(1000),
                new ExceptionTemplateRequiredDescriptionRule("exception template needs a value"),
                new ExceptionTemplateCurrencyLengthRule(80),
                new ExceptionTemplateTitleLengthRule(86400),
            });
    }
}
