namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>A rule a retro pay cycle must pass. Check returns its problems, or none.</summary>
public interface IRetroPayCycleRule
{
    List<string> Check(RetroPayCycle item);
}

public sealed record RetroPayCycleRequiredCodeRule(string Message) : IRetroPayCycleRule
{
    public List<string> Check(RetroPayCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayCycleCodeLengthRule(int MaxLength) : IRetroPayCycleRule
{
    public List<string> Check(RetroPayCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayCycleExpiresAtRangeRule(int Min, int Max) : IRetroPayCycleRule
{
    public List<string> Check(RetroPayCycle item)
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

/// <summary>Runs every rule of a retro pay cycle.</summary>
public sealed record RetroPayCycleValidator(List<IRetroPayCycleRule> Rules)
{
    public List<string> Validate(RetroPayCycle item)
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

    public void RequireValid(RetroPayCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayCycle> items)
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

/// <summary>Functions of retro pay cycle rules.</summary>
public static class RetroPayCycleRules
{
    public static RetroPayCycleValidator DefaultRetroPayCycleValidator()
    {
        return new RetroPayCycleValidator(
            new List<IRetroPayCycleRule>
            {
                new RetroPayCycleRequiredCodeRule("retro pay cycle needs an entry"),
                new RetroPayCycleCodeLengthRule(16),
                new RetroPayCycleExpiresAtRangeRule(80, 1000),
            });
    }
}
