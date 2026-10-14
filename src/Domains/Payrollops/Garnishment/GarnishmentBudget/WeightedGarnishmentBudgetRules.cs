namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

/// <summary>A rule a weighted garnishment budget must pass. Check returns its problems, or none.</summary>
public interface IWeightedGarnishmentBudgetRule
{
    List<string> Check(WeightedGarnishmentBudget item);
}

public sealed record WeightedGarnishmentBudgetRequiredLabelRule(string Message) : IWeightedGarnishmentBudgetRule
{
    public List<string> Check(WeightedGarnishmentBudget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedGarnishmentBudgetAmountRangeRule(double Min, double Max) : IWeightedGarnishmentBudgetRule
{
    public List<string> Check(WeightedGarnishmentBudget item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedGarnishmentBudgetApprovedGuardRule(double Limit) : IWeightedGarnishmentBudgetRule
{
    public List<string> Check(WeightedGarnishmentBudget item)
    {
        if (item.Approved && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record WeightedGarnishmentBudgetJobCodeLengthRule(int MaxLength) : IWeightedGarnishmentBudgetRule
{
    public List<string> Check(WeightedGarnishmentBudget item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedGarnishmentBudgetScoreRangeRule(double Min, double Max) : IWeightedGarnishmentBudgetRule
{
    public List<string> Check(WeightedGarnishmentBudget item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted garnishment budget.</summary>
public sealed record WeightedGarnishmentBudgetValidator(List<IWeightedGarnishmentBudgetRule> Rules)
{
    public List<string> Validate(WeightedGarnishmentBudget item)
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

    public void RequireValid(WeightedGarnishmentBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedGarnishmentBudget> items)
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

/// <summary>Functions of weighted garnishment budget rules.</summary>
public static class WeightedGarnishmentBudgetRules
{
    public static WeightedGarnishmentBudgetValidator DefaultWeightedGarnishmentBudgetValidator()
    {
        return new WeightedGarnishmentBudgetValidator(
            new List<IWeightedGarnishmentBudgetRule>
            {
                new WeightedGarnishmentBudgetRequiredLabelRule("weighted garnishment budget needs an entry"),
                new WeightedGarnishmentBudgetAmountRangeRule(2.2, 1371.6),
                new WeightedGarnishmentBudgetApprovedGuardRule(1.5),
                new WeightedGarnishmentBudgetJobCodeLengthRule(16),
                new WeightedGarnishmentBudgetScoreRangeRule(7.9, 3689.8),
            });
    }
}
