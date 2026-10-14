namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>A rule a retro pay score must pass. Check returns its problems, or none.</summary>
public interface IRetroPayScoreRule
{
    List<string> Check(RetroPayScore item);
}

public sealed record RetroPayScoreCodeLengthRule(int MaxLength) : IRetroPayScoreRule
{
    public List<string> Check(RetroPayScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayScoreCapacityRangeRule(int Min, int Max) : IRetroPayScoreRule
{
    public List<string> Check(RetroPayScore item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayScoreBadgeLengthRule(int MaxLength) : IRetroPayScoreRule
{
    public List<string> Check(RetroPayScore item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayScoreRequiredCodeRule(string Message) : IRetroPayScoreRule
{
    public List<string> Check(RetroPayScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay score.</summary>
public sealed record RetroPayScoreValidator(List<IRetroPayScoreRule> Rules)
{
    public List<string> Validate(RetroPayScore item)
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

    public void RequireValid(RetroPayScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayScore> items)
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

/// <summary>Functions of retro pay score rules.</summary>
public static class RetroPayScoreRules
{
    public static RetroPayScoreValidator DefaultRetroPayScoreValidator()
    {
        return new RetroPayScoreValidator(
            new List<IRetroPayScoreRule>
            {
                new RetroPayScoreCodeLengthRule(32),
                new RetroPayScoreCapacityRangeRule(32, 3600),
                new RetroPayScoreBadgeLengthRule(80),
                new RetroPayScoreRequiredCodeRule("retro pay score needs this field"),
            });
    }
}
