namespace Acme.Workforce.Absence.Balance.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a balance request must pass. Check returns its problems, or none.</summary>
public interface IBalanceRequestRule
{
    List<string> Check(BalanceRequest item);
}

public sealed record BalanceRequestAllowedNotesRule(List<string> Allowed) : IBalanceRequestRule
{
    public List<string> Check(BalanceRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is expired" };
        }
        return new List<string>();
    }
}

public sealed record BalanceRequestNameLengthRule(int MaxLength) : IBalanceRequestRule
{
    public List<string> Check(BalanceRequest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceRequestLimitRangeRule(int Min, int Max) : IBalanceRequestRule
{
    public List<string> Check(BalanceRequest item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceRequestBudgetRangeRule(double Min, double Max) : IBalanceRequestRule
{
    public List<string> Check(BalanceRequest item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance request.</summary>
public sealed record BalanceRequestValidator(List<IBalanceRequestRule> Rules)
{
    public List<string> Validate(BalanceRequest item)
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

    public void RequireValid(BalanceRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceRequest> items)
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

/// <summary>Functions of balance request rules.</summary>
public static class BalanceRequestRules
{
    public static BalanceRequestValidator DefaultBalanceRequestValidator()
    {
        return new BalanceRequestValidator(
            new List<IBalanceRequestRule>
            {
                new BalanceRequestAllowedNotesRule(new List<string>()),
                new BalanceRequestNameLengthRule(120),
                new BalanceRequestLimitRangeRule(16, 64),
                new BalanceRequestBudgetRangeRule(0.4, 1256.9),
            });
    }
}
