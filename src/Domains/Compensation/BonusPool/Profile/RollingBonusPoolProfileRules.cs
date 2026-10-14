namespace Acme.Workforce.Compensation.BonusPool.Profile;

/// <summary>A rule a rolling bonus pool profile must pass. Check returns its problems, or none.</summary>
public interface IRollingBonusPoolProfileRule
{
    List<string> Check(RollingBonusPoolProfile item);
}

public sealed record RollingBonusPoolProfileSummaryLengthRule(int MaxLength) : IRollingBonusPoolProfileRule
{
    public List<string> Check(RollingBonusPoolProfile item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBonusPoolProfileRequiredNameRule(string Message) : IRollingBonusPoolProfileRule
{
    public List<string> Check(RollingBonusPoolProfile item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingBonusPoolProfileAmountRangeRule(double Min, double Max) : IRollingBonusPoolProfileRule
{
    public List<string> Check(RollingBonusPoolProfile item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingBonusPoolProfileCurrencyLengthRule(int MaxLength) : IRollingBonusPoolProfileRule
{
    public List<string> Check(RollingBonusPoolProfile item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling bonus pool profile.</summary>
public sealed record RollingBonusPoolProfileValidator(List<IRollingBonusPoolProfileRule> Rules)
{
    public List<string> Validate(RollingBonusPoolProfile item)
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

    public void RequireValid(RollingBonusPoolProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingBonusPoolProfile> items)
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

/// <summary>Functions of rolling bonus pool profile rules.</summary>
public static class RollingBonusPoolProfileRules
{
    public static RollingBonusPoolProfileValidator DefaultRollingBonusPoolProfileValidator()
    {
        return new RollingBonusPoolProfileValidator(
            new List<IRollingBonusPoolProfileRule>
            {
                new RollingBonusPoolProfileSummaryLengthRule(80),
                new RollingBonusPoolProfileRequiredNameRule("rolling bonus pool profile needs this field"),
                new RollingBonusPoolProfileAmountRangeRule(5.4, 2452.3),
                new RollingBonusPoolProfileCurrencyLengthRule(64),
            });
    }
}
