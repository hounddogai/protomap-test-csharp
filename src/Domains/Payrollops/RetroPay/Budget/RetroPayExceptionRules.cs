namespace Acme.Workforce.Payrollops.RetroPay.Budget;

/// <summary>A rule a retro pay exception must pass. Check returns its problems, or none.</summary>
public interface IRetroPayExceptionRule
{
    List<string> Check(RetroPayException item);
}

public sealed record RetroPayExceptionRequiredCodeRule(string Message) : IRetroPayExceptionRule
{
    public List<string> Check(RetroPayException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayExceptionRequiredGradeRule(string Message) : IRetroPayExceptionRule
{
    public List<string> Check(RetroPayException item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayExceptionCeilingRangeRule(double Min, double Max) : IRetroPayExceptionRule
{
    public List<string> Check(RetroPayException item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RetroPayExceptionQuantityRangeRule(int Min, int Max) : IRetroPayExceptionRule
{
    public List<string> Check(RetroPayException item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RetroPayExceptionRequiredLabelRule(string Message) : IRetroPayExceptionRule
{
    public List<string> Check(RetroPayException item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayExceptionCountryLengthRule(int MaxLength) : IRetroPayExceptionRule
{
    public List<string> Check(RetroPayException item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay exception.</summary>
public sealed record RetroPayExceptionValidator(List<IRetroPayExceptionRule> Rules)
{
    public List<string> Validate(RetroPayException item)
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

    public void RequireValid(RetroPayException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayException> items)
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

/// <summary>Functions of retro pay exception rules.</summary>
public static class RetroPayExceptionRules
{
    public static RetroPayExceptionValidator DefaultRetroPayExceptionValidator()
    {
        return new RetroPayExceptionValidator(
            new List<IRetroPayExceptionRule>
            {
                new RetroPayExceptionRequiredCodeRule("retro pay exception needs this field"),
                new RetroPayExceptionRequiredGradeRule("retro pay exception needs a value"),
                new RetroPayExceptionCeilingRangeRule(0.4, 2870.9),
                new RetroPayExceptionQuantityRangeRule(80, 80),
                new RetroPayExceptionRequiredLabelRule("retro pay exception needs a value"),
                new RetroPayExceptionCountryLengthRule(80),
            });
    }
}
