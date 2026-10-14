namespace Acme.Workforce.Payrollops.RetroPay.Window;

/// <summary>A rule a retro pay window must pass. Check returns its problems, or none.</summary>
public interface IRetroPayWindowRule
{
    List<string> Check(RetroPayWindow item);
}

public sealed record RetroPayWindowLabelLengthRule(int MaxLength) : IRetroPayWindowRule
{
    public List<string> Check(RetroPayWindow item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayWindowRequiredSourceRule(string Message) : IRetroPayWindowRule
{
    public List<string> Check(RetroPayWindow item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayWindowExpiresAtRangeRule(int Min, int Max) : IRetroPayWindowRule
{
    public List<string> Check(RetroPayWindow item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RetroPayWindowRequiredSalaryBandRule(string Message) : IRetroPayWindowRule
{
    public List<string> Check(RetroPayWindow item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay window.</summary>
public sealed record RetroPayWindowValidator(List<IRetroPayWindowRule> Rules)
{
    public List<string> Validate(RetroPayWindow item)
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

    public void RequireValid(RetroPayWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayWindow> items)
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

/// <summary>Functions of retro pay window rules.</summary>
public static class RetroPayWindowRules
{
    public static RetroPayWindowValidator DefaultRetroPayWindowValidator()
    {
        return new RetroPayWindowValidator(
            new List<IRetroPayWindowRule>
            {
                new RetroPayWindowLabelLengthRule(16),
                new RetroPayWindowRequiredSourceRule("retro pay window needs an entry"),
                new RetroPayWindowExpiresAtRangeRule(64, 1000),
                new RetroPayWindowRequiredSalaryBandRule("retro pay window needs an entry"),
            });
    }
}
