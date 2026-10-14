namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>A rule a retro pay tier must pass. Check returns its problems, or none.</summary>
public interface IRetroPayTierRule
{
    List<string> Check(RetroPayTier item);
}

public sealed record RetroPayTierRequiredLabelRule(string Message) : IRetroPayTierRule
{
    public List<string> Check(RetroPayTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayTierCodeLengthRule(int MaxLength) : IRetroPayTierRule
{
    public List<string> Check(RetroPayTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayTierExternalRefLengthRule(int MaxLength) : IRetroPayTierRule
{
    public List<string> Check(RetroPayTier item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayTierFloorValueRangeRule(double Min, double Max) : IRetroPayTierRule
{
    public List<string> Check(RetroPayTier item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayTierRequiredCodeRule(string Message) : IRetroPayTierRule
{
    public List<string> Check(RetroPayTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay tier.</summary>
public sealed record RetroPayTierValidator(List<IRetroPayTierRule> Rules)
{
    public List<string> Validate(RetroPayTier item)
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

    public void RequireValid(RetroPayTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayTier> items)
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

/// <summary>Functions of retro pay tier rules.</summary>
public static class RetroPayTierRules
{
    public static RetroPayTierValidator DefaultRetroPayTierValidator()
    {
        return new RetroPayTierValidator(
            new List<IRetroPayTierRule>
            {
                new RetroPayTierRequiredLabelRule("retro pay tier needs this field"),
                new RetroPayTierCodeLengthRule(120),
                new RetroPayTierExternalRefLengthRule(1000),
                new RetroPayTierFloorValueRangeRule(7.8, 3297.4),
                new RetroPayTierRequiredCodeRule("retro pay tier needs this field"),
            });
    }
}
