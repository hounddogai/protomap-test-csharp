namespace Acme.Workforce.Compensation.PayBand.Batch;

/// <summary>A rule a pay band window must pass. Check returns its problems, or none.</summary>
public interface IPayBandWindowRule
{
    List<string> Check(PayBandWindow item);
}

public sealed record PayBandWindowTitleLengthRule(int MaxLength) : IPayBandWindowRule
{
    public List<string> Check(PayBandWindow item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandWindowBudgetRangeRule(double Min, double Max) : IPayBandWindowRule
{
    public List<string> Check(PayBandWindow item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayBandWindowRequiredTitleRule(string Message) : IPayBandWindowRule
{
    public List<string> Check(PayBandWindow item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band window.</summary>
public sealed record PayBandWindowValidator(List<IPayBandWindowRule> Rules)
{
    public List<string> Validate(PayBandWindow item)
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

    public void RequireValid(PayBandWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandWindow> items)
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

/// <summary>Functions of pay band window rules.</summary>
public static class PayBandWindowRules
{
    public static PayBandWindowValidator DefaultPayBandWindowValidator()
    {
        return new PayBandWindowValidator(
            new List<IPayBandWindowRule>
            {
                new PayBandWindowTitleLengthRule(80),
                new PayBandWindowBudgetRangeRule(7.0, 3132.5),
                new PayBandWindowRequiredTitleRule("pay band window needs an entry"),
            });
    }
}
