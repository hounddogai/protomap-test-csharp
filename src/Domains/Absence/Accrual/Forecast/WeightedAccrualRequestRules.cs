namespace Acme.Workforce.Absence.Accrual.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted accrual request must pass. Check returns its problems, or none.</summary>
public interface IWeightedAccrualRequestRule
{
    List<string> Check(WeightedAccrualRequest item);
}

public sealed record WeightedAccrualRequestCreatedAtRangeRule(int Min, int Max) : IWeightedAccrualRequestRule
{
    public List<string> Check(WeightedAccrualRequest item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedAccrualRequestRequiredChecksumRule(string Message) : IWeightedAccrualRequestRule
{
    public List<string> Check(WeightedAccrualRequest item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccrualRequestTitleLengthRule(int MaxLength) : IWeightedAccrualRequestRule
{
    public List<string> Check(WeightedAccrualRequest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccrualRequestAllowedTitleRule(List<string> Allowed) : IWeightedAccrualRequestRule
{
    public List<string> Check(WeightedAccrualRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted accrual request.</summary>
public sealed record WeightedAccrualRequestValidator(List<IWeightedAccrualRequestRule> Rules)
{
    public List<string> Validate(WeightedAccrualRequest item)
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

    public void RequireValid(WeightedAccrualRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedAccrualRequest> items)
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

/// <summary>Functions of weighted accrual request rules.</summary>
public static class WeightedAccrualRequestRules
{
    public static WeightedAccrualRequestValidator DefaultWeightedAccrualRequestValidator()
    {
        return new WeightedAccrualRequestValidator(
            new List<IWeightedAccrualRequestRule>
            {
                new WeightedAccrualRequestCreatedAtRangeRule(64, 3600),
                new WeightedAccrualRequestRequiredChecksumRule("weighted accrual request needs an entry"),
                new WeightedAccrualRequestTitleLengthRule(3600),
                new WeightedAccrualRequestAllowedTitleRule(new List<string>()),
            });
    }
}
