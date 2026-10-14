namespace Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>A rule a pay run window must pass. Check returns its problems, or none.</summary>
public interface IPayRunWindowRule
{
    List<string> Check(PayRunWindow item);
}

public sealed record PayRunWindowCategoryLengthRule(int MaxLength) : IPayRunWindowRule
{
    public List<string> Check(PayRunWindow item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunWindowReviewerLengthRule(int MaxLength) : IPayRunWindowRule
{
    public List<string> Check(PayRunWindow item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunWindowRequiredLabelRule(string Message) : IPayRunWindowRule
{
    public List<string> Check(PayRunWindow item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunWindowFloorValueRangeRule(double Min, double Max) : IPayRunWindowRule
{
    public List<string> Check(PayRunWindow item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunWindowRequiredCodeRule(string Message) : IPayRunWindowRule
{
    public List<string> Check(PayRunWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run window.</summary>
public sealed record PayRunWindowValidator(List<IPayRunWindowRule> Rules)
{
    public List<string> Validate(PayRunWindow item)
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

    public void RequireValid(PayRunWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunWindow> items)
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

/// <summary>Functions of pay run window rules.</summary>
public static class PayRunWindowRules
{
    public static PayRunWindowValidator DefaultPayRunWindowValidator()
    {
        return new PayRunWindowValidator(
            new List<IPayRunWindowRule>
            {
                new PayRunWindowCategoryLengthRule(32),
                new PayRunWindowReviewerLengthRule(3600),
                new PayRunWindowRequiredLabelRule("pay run window needs a value"),
                new PayRunWindowFloorValueRangeRule(9.5, 3135.8),
                new PayRunWindowRequiredCodeRule("pay run window needs an entry"),
            });
    }
}
