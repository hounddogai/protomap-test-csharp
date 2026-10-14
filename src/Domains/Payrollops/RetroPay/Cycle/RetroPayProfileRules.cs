namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>A rule a retro pay profile must pass. Check returns its problems, or none.</summary>
public interface IRetroPayProfileRule
{
    List<string> Check(RetroPayProfile item);
}

public sealed record RetroPayProfileCodePrefixRule(string Prefix) : IRetroPayProfileRule
{
    public List<string> Check(RetroPayProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayProfileWeightRangeRule(double Min, double Max) : IRetroPayProfileRule
{
    public List<string> Check(RetroPayProfile item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RetroPayProfileCodeLengthRule(int MaxLength) : IRetroPayProfileRule
{
    public List<string> Check(RetroPayProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayProfileRequiredCodeRule(string Message) : IRetroPayProfileRule
{
    public List<string> Check(RetroPayProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayProfileCeilingRangeRule(double Min, double Max) : IRetroPayProfileRule
{
    public List<string> Check(RetroPayProfile item)
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

public sealed record RetroPayProfilePriorityRangeRule(int Min, int Max) : IRetroPayProfileRule
{
    public List<string> Check(RetroPayProfile item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay profile.</summary>
public sealed record RetroPayProfileValidator(List<IRetroPayProfileRule> Rules)
{
    public List<string> Validate(RetroPayProfile item)
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

    public void RequireValid(RetroPayProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayProfile> items)
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

/// <summary>Functions of retro pay profile rules.</summary>
public static class RetroPayProfileRules
{
    public static RetroPayProfileValidator DefaultRetroPayProfileValidator()
    {
        return new RetroPayProfileValidator(
            new List<IRetroPayProfileRule>
            {
                new RetroPayProfileCodePrefixRule("ACM"),
                new RetroPayProfileWeightRangeRule(2.4, 1209.1),
                new RetroPayProfileCodeLengthRule(3600),
                new RetroPayProfileRequiredCodeRule("retro pay profile needs this field"),
                new RetroPayProfileCeilingRangeRule(0.5, 4290.6),
                new RetroPayProfilePriorityRangeRule(32, 86400),
            });
    }
}
