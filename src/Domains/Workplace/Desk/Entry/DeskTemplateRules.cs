namespace Acme.Workforce.Workplace.Desk.Entry;

/// <summary>A rule a desk template must pass. Check returns its problems, or none.</summary>
public interface IDeskTemplateRule
{
    List<string> Check(DeskTemplate item);
}

public sealed record DeskTemplateRetriesRangeRule(int Min, int Max) : IDeskTemplateRule
{
    public List<string> Check(DeskTemplate item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeskTemplateMarginRangeRule(double Min, double Max) : IDeskTemplateRule
{
    public List<string> Check(DeskTemplate item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeskTemplateRatioRangeRule(double Min, double Max) : IDeskTemplateRule
{
    public List<string> Check(DeskTemplate item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeskTemplateDescriptionLengthRule(int MaxLength) : IDeskTemplateRule
{
    public List<string> Check(DeskTemplate item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskTemplateRequiredCountryRule(string Message) : IDeskTemplateRule
{
    public List<string> Check(DeskTemplate item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskTemplateCountryLengthRule(int MaxLength) : IDeskTemplateRule
{
    public List<string> Check(DeskTemplate item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk template.</summary>
public sealed record DeskTemplateValidator(List<IDeskTemplateRule> Rules)
{
    public List<string> Validate(DeskTemplate item)
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

    public void RequireValid(DeskTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskTemplate> items)
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

/// <summary>Functions of desk template rules.</summary>
public static class DeskTemplateRules
{
    public static DeskTemplateValidator DefaultDeskTemplateValidator()
    {
        return new DeskTemplateValidator(
            new List<IDeskTemplateRule>
            {
                new DeskTemplateRetriesRangeRule(80, 120),
                new DeskTemplateMarginRangeRule(9.7, 2356.3),
                new DeskTemplateRatioRangeRule(7.6, 4451.6),
                new DeskTemplateDescriptionLengthRule(120),
                new DeskTemplateRequiredCountryRule("desk template needs a value"),
                new DeskTemplateCountryLengthRule(255),
            });
    }
}
