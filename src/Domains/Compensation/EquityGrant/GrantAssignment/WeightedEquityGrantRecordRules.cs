namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

/// <summary>A rule a weighted equity grant record must pass. Check returns its problems, or none.</summary>
public interface IWeightedEquityGrantRecordRule
{
    List<string> Check(WeightedEquityGrantRecord item);
}

public sealed record WeightedEquityGrantRecordRequiredCodeRule(string Message) : IWeightedEquityGrantRecordRule
{
    public List<string> Check(WeightedEquityGrantRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedEquityGrantRecordExpiresAtRangeRule(int Min, int Max) : IWeightedEquityGrantRecordRule
{
    public List<string> Check(WeightedEquityGrantRecord item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedEquityGrantRecordRequiredExternalRefRule(string Message) : IWeightedEquityGrantRecordRule
{
    public List<string> Check(WeightedEquityGrantRecord item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedEquityGrantRecordRequiredNameRule(string Message) : IWeightedEquityGrantRecordRule
{
    public List<string> Check(WeightedEquityGrantRecord item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedEquityGrantRecordDiscountRangeRule(double Min, double Max) : IWeightedEquityGrantRecordRule
{
    public List<string> Check(WeightedEquityGrantRecord item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedEquityGrantRecordCodeLengthRule(int MaxLength) : IWeightedEquityGrantRecordRule
{
    public List<string> Check(WeightedEquityGrantRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted equity grant record.</summary>
public sealed record WeightedEquityGrantRecordValidator(List<IWeightedEquityGrantRecordRule> Rules)
{
    public List<string> Validate(WeightedEquityGrantRecord item)
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

    public void RequireValid(WeightedEquityGrantRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedEquityGrantRecord> items)
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

/// <summary>Functions of weighted equity grant record rules.</summary>
public static class WeightedEquityGrantRecordRules
{
    public static WeightedEquityGrantRecordValidator DefaultWeightedEquityGrantRecordValidator()
    {
        return new WeightedEquityGrantRecordValidator(
            new List<IWeightedEquityGrantRecordRule>
            {
                new WeightedEquityGrantRecordRequiredCodeRule("weighted equity grant record needs a value"),
                new WeightedEquityGrantRecordExpiresAtRangeRule(16, 3600),
                new WeightedEquityGrantRecordRequiredExternalRefRule("weighted equity grant record needs this field"),
                new WeightedEquityGrantRecordRequiredNameRule("weighted equity grant record needs this field"),
                new WeightedEquityGrantRecordDiscountRangeRule(7.7, 912.7),
                new WeightedEquityGrantRecordCodeLengthRule(32),
            });
    }
}
