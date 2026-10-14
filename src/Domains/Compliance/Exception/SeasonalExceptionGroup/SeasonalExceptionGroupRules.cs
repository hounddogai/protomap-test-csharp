namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>A rule a seasonal exception group must pass. Check returns its problems, or none.</summary>
public interface ISeasonalExceptionGroupRule
{
    List<string> Check(SeasonalExceptionGroup item);
}

public sealed record SeasonalExceptionGroupDiscountRangeRule(double Min, double Max) : ISeasonalExceptionGroupRule
{
    public List<string> Check(SeasonalExceptionGroup item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalExceptionGroupCodeLengthRule(int MaxLength) : ISeasonalExceptionGroupRule
{
    public List<string> Check(SeasonalExceptionGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalExceptionGroupRequiredLabelRule(string Message) : ISeasonalExceptionGroupRule
{
    public List<string> Check(SeasonalExceptionGroup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalExceptionGroupManagerIdLengthRule(int MaxLength) : ISeasonalExceptionGroupRule
{
    public List<string> Check(SeasonalExceptionGroup item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalExceptionGroupRequiredCodeRule(string Message) : ISeasonalExceptionGroupRule
{
    public List<string> Check(SeasonalExceptionGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal exception group.</summary>
public sealed record SeasonalExceptionGroupValidator(List<ISeasonalExceptionGroupRule> Rules)
{
    public List<string> Validate(SeasonalExceptionGroup item)
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

    public void RequireValid(SeasonalExceptionGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalExceptionGroup> items)
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

/// <summary>Functions of seasonal exception group rules.</summary>
public static class SeasonalExceptionGroupRules
{
    public static SeasonalExceptionGroupValidator DefaultSeasonalExceptionGroupValidator()
    {
        return new SeasonalExceptionGroupValidator(
            new List<ISeasonalExceptionGroupRule>
            {
                new SeasonalExceptionGroupDiscountRangeRule(8.7, 647.4),
                new SeasonalExceptionGroupCodeLengthRule(32),
                new SeasonalExceptionGroupRequiredLabelRule("seasonal exception group needs this field"),
                new SeasonalExceptionGroupManagerIdLengthRule(3600),
                new SeasonalExceptionGroupRequiredCodeRule("seasonal exception group needs this field"),
            });
    }
}
