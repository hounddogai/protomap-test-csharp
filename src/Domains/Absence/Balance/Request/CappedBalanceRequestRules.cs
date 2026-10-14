namespace Acme.Workforce.Absence.Balance.Request;

/// <summary>A rule a capped balance request must pass. Check returns its problems, or none.</summary>
public interface ICappedBalanceRequestRule
{
    List<string> Check(CappedBalanceRequest item);
}

public sealed record CappedBalanceRequestFteRangeRule(double Min, double Max) : ICappedBalanceRequestRule
{
    public List<string> Check(CappedBalanceRequest item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedBalanceRequestQuantityRangeRule(int Min, int Max) : ICappedBalanceRequestRule
{
    public List<string> Check(CappedBalanceRequest item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedBalanceRequestMaximumRangeRule(double Min, double Max) : ICappedBalanceRequestRule
{
    public List<string> Check(CappedBalanceRequest item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedBalanceRequestDescriptionLengthRule(int MaxLength) : ICappedBalanceRequestRule
{
    public List<string> Check(CappedBalanceRequest item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped balance request.</summary>
public sealed record CappedBalanceRequestValidator(List<ICappedBalanceRequestRule> Rules)
{
    public List<string> Validate(CappedBalanceRequest item)
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

    public void RequireValid(CappedBalanceRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedBalanceRequest> items)
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

/// <summary>Functions of capped balance request rules.</summary>
public static class CappedBalanceRequestRules
{
    public static CappedBalanceRequestValidator DefaultCappedBalanceRequestValidator()
    {
        return new CappedBalanceRequestValidator(
            new List<ICappedBalanceRequestRule>
            {
                new CappedBalanceRequestFteRangeRule(1.1, 2707.4),
                new CappedBalanceRequestQuantityRangeRule(16, 3600),
                new CappedBalanceRequestMaximumRangeRule(4.5, 4526.4),
                new CappedBalanceRequestDescriptionLengthRule(80),
            });
    }
}
