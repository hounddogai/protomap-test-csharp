namespace Acme.Workforce.Payrollops.PayRun.Checkpoint;

/// <summary>A rule a pay run template must pass. Check returns its problems, or none.</summary>
public interface IPayRunTemplateRule
{
    List<string> Check(PayRunTemplate item);
}

public sealed record PayRunTemplateRevisionRangeRule(int Min, int Max) : IPayRunTemplateRule
{
    public List<string> Check(PayRunTemplate item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayRunTemplateRequiredContactEmailRule(string Message) : IPayRunTemplateRule
{
    public List<string> Check(PayRunTemplate item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunTemplateLabelLengthRule(int MaxLength) : IPayRunTemplateRule
{
    public List<string> Check(PayRunTemplate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunTemplateSummaryLengthRule(int MaxLength) : IPayRunTemplateRule
{
    public List<string> Check(PayRunTemplate item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run template.</summary>
public sealed record PayRunTemplateValidator(List<IPayRunTemplateRule> Rules)
{
    public List<string> Validate(PayRunTemplate item)
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

    public void RequireValid(PayRunTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunTemplate> items)
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

/// <summary>Functions of pay run template rules.</summary>
public static class PayRunTemplateRules
{
    public static PayRunTemplateValidator DefaultPayRunTemplateValidator()
    {
        return new PayRunTemplateValidator(
            new List<IPayRunTemplateRule>
            {
                new PayRunTemplateRevisionRangeRule(120, 3600),
                new PayRunTemplateRequiredContactEmailRule("pay run template needs a value"),
                new PayRunTemplateLabelLengthRule(32),
                new PayRunTemplateSummaryLengthRule(32),
            });
    }
}
