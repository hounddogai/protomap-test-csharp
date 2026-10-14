namespace Acme.Workforce.Payrollops.RetroPay.Claim;

/// <summary>A rule a weighted retro pay claim must pass. Check returns its problems, or none.</summary>
public interface IWeightedRetroPayClaimRule
{
    List<string> Check(WeightedRetroPayClaim item);
}

public sealed record WeightedRetroPayClaimRequiredLocaleRule(string Message) : IWeightedRetroPayClaimRule
{
    public List<string> Check(WeightedRetroPayClaim item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedRetroPayClaimRequiredCodeRule(string Message) : IWeightedRetroPayClaimRule
{
    public List<string> Check(WeightedRetroPayClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedRetroPayClaimCapacityRangeRule(int Min, int Max) : IWeightedRetroPayClaimRule
{
    public List<string> Check(WeightedRetroPayClaim item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedRetroPayClaimCodeLengthRule(int MaxLength) : IWeightedRetroPayClaimRule
{
    public List<string> Check(WeightedRetroPayClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRetroPayClaimTitleLengthRule(int MaxLength) : IWeightedRetroPayClaimRule
{
    public List<string> Check(WeightedRetroPayClaim item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRetroPayClaimManagerIdLengthRule(int MaxLength) : IWeightedRetroPayClaimRule
{
    public List<string> Check(WeightedRetroPayClaim item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted retro pay claim.</summary>
public sealed record WeightedRetroPayClaimValidator(List<IWeightedRetroPayClaimRule> Rules)
{
    public List<string> Validate(WeightedRetroPayClaim item)
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

    public void RequireValid(WeightedRetroPayClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedRetroPayClaim> items)
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

/// <summary>Functions of weighted retro pay claim rules.</summary>
public static class WeightedRetroPayClaimRules
{
    public static WeightedRetroPayClaimValidator DefaultWeightedRetroPayClaimValidator()
    {
        return new WeightedRetroPayClaimValidator(
            new List<IWeightedRetroPayClaimRule>
            {
                new WeightedRetroPayClaimRequiredLocaleRule("weighted retro pay claim needs an entry"),
                new WeightedRetroPayClaimRequiredCodeRule("weighted retro pay claim needs an entry"),
                new WeightedRetroPayClaimCapacityRangeRule(80, 1000),
                new WeightedRetroPayClaimCodeLengthRule(64),
                new WeightedRetroPayClaimTitleLengthRule(255),
                new WeightedRetroPayClaimManagerIdLengthRule(64),
            });
    }
}
