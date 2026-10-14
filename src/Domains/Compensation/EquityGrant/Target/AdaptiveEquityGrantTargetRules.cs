namespace Acme.Workforce.Compensation.EquityGrant.Target;

/// <summary>A rule a adaptive equity grant target must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveEquityGrantTargetRule
{
    List<string> Check(AdaptiveEquityGrantTarget item);
}

public sealed record AdaptiveEquityGrantTargetRequiredCountryRule(string Message) : IAdaptiveEquityGrantTargetRule
{
    public List<string> Check(AdaptiveEquityGrantTarget item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEquityGrantTargetCodeLengthRule(int MaxLength) : IAdaptiveEquityGrantTargetRule
{
    public List<string> Check(AdaptiveEquityGrantTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEquityGrantTargetNameLengthRule(int MaxLength) : IAdaptiveEquityGrantTargetRule
{
    public List<string> Check(AdaptiveEquityGrantTarget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEquityGrantTargetPriorityRangeRule(int Min, int Max) : IAdaptiveEquityGrantTargetRule
{
    public List<string> Check(AdaptiveEquityGrantTarget item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive equity grant target.</summary>
public sealed record AdaptiveEquityGrantTargetValidator(List<IAdaptiveEquityGrantTargetRule> Rules)
{
    public List<string> Validate(AdaptiveEquityGrantTarget item)
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

    public void RequireValid(AdaptiveEquityGrantTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveEquityGrantTarget> items)
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

/// <summary>Functions of adaptive equity grant target rules.</summary>
public static class AdaptiveEquityGrantTargetRules
{
    public static AdaptiveEquityGrantTargetValidator DefaultAdaptiveEquityGrantTargetValidator()
    {
        return new AdaptiveEquityGrantTargetValidator(
            new List<IAdaptiveEquityGrantTargetRule>
            {
                new AdaptiveEquityGrantTargetRequiredCountryRule("adaptive equity grant target needs an entry"),
                new AdaptiveEquityGrantTargetCodeLengthRule(120),
                new AdaptiveEquityGrantTargetNameLengthRule(80),
                new AdaptiveEquityGrantTargetPriorityRangeRule(32, 64),
            });
    }
}
