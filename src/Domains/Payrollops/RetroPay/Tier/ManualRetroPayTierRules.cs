namespace Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>A rule a manual retro pay tier must pass. Check returns its problems, or none.</summary>
public interface IManualRetroPayTierRule
{
    List<string> Check(ManualRetroPayTier item);
}

public sealed record ManualRetroPayTierRequiredCodeRule(string Message) : IManualRetroPayTierRule
{
    public List<string> Check(ManualRetroPayTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualRetroPayTierRequiredNameRule(string Message) : IManualRetroPayTierRule
{
    public List<string> Check(ManualRetroPayTier item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualRetroPayTierCodePrefixRule(string Prefix) : IManualRetroPayTierRule
{
    public List<string> Check(ManualRetroPayTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualRetroPayTierManagerIdLengthRule(int MaxLength) : IManualRetroPayTierRule
{
    public List<string> Check(ManualRetroPayTier item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual retro pay tier.</summary>
public sealed record ManualRetroPayTierValidator(List<IManualRetroPayTierRule> Rules)
{
    public List<string> Validate(ManualRetroPayTier item)
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

    public void RequireValid(ManualRetroPayTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualRetroPayTier> items)
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

/// <summary>Functions of manual retro pay tier rules.</summary>
public static class ManualRetroPayTierRules
{
    public static ManualRetroPayTierValidator DefaultManualRetroPayTierValidator()
    {
        return new ManualRetroPayTierValidator(
            new List<IManualRetroPayTierRule>
            {
                new ManualRetroPayTierRequiredCodeRule("manual retro pay tier needs a value"),
                new ManualRetroPayTierRequiredNameRule("manual retro pay tier needs a value"),
                new ManualRetroPayTierCodePrefixRule("Q"),
                new ManualRetroPayTierManagerIdLengthRule(64),
            });
    }
}
