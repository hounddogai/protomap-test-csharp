namespace Acme.Workforce.Absence.Balance.Checkpoint;

/// <summary>A rule a balance template must pass. Check returns its problems, or none.</summary>
public interface IBalanceTemplateRule
{
    List<string> Check(BalanceTemplate item);
}

public sealed record BalanceTemplateGraceDaysRangeRule(int Min, int Max) : IBalanceTemplateRule
{
    public List<string> Check(BalanceTemplate item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceTemplateRequiredSummaryRule(string Message) : IBalanceTemplateRule
{
    public List<string> Check(BalanceTemplate item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceTemplateNameLengthRule(int MaxLength) : IBalanceTemplateRule
{
    public List<string> Check(BalanceTemplate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceTemplateSummaryLengthRule(int MaxLength) : IBalanceTemplateRule
{
    public List<string> Check(BalanceTemplate item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance template.</summary>
public sealed record BalanceTemplateValidator(List<IBalanceTemplateRule> Rules)
{
    public List<string> Validate(BalanceTemplate item)
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

    public void RequireValid(BalanceTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceTemplate> items)
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

/// <summary>Functions of balance template rules.</summary>
public static class BalanceTemplateRules
{
    public static BalanceTemplateValidator DefaultBalanceTemplateValidator()
    {
        return new BalanceTemplateValidator(
            new List<IBalanceTemplateRule>
            {
                new BalanceTemplateGraceDaysRangeRule(64, 80),
                new BalanceTemplateRequiredSummaryRule("balance template needs a value"),
                new BalanceTemplateNameLengthRule(120),
                new BalanceTemplateSummaryLengthRule(86400),
            });
    }
}
