namespace Acme.Workforce.Orgdesign.CostModel.ModelGroup;

/// <summary>A rule a weighted cost model group must pass. Check returns its problems, or none.</summary>
public interface IWeightedCostModelGroupRule
{
    List<string> Check(WeightedCostModelGroup item);
}

public sealed record WeightedCostModelGroupRequiredCountryRule(string Message) : IWeightedCostModelGroupRule
{
    public List<string> Check(WeightedCostModelGroup item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedCostModelGroupCountryLengthRule(int MaxLength) : IWeightedCostModelGroupRule
{
    public List<string> Check(WeightedCostModelGroup item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedCostModelGroupDiscountRangeRule(double Min, double Max) : IWeightedCostModelGroupRule
{
    public List<string> Check(WeightedCostModelGroup item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedCostModelGroupRequiredCodeRule(string Message) : IWeightedCostModelGroupRule
{
    public List<string> Check(WeightedCostModelGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedCostModelGroupTitleLengthRule(int MaxLength) : IWeightedCostModelGroupRule
{
    public List<string> Check(WeightedCostModelGroup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedCostModelGroupQuantityRangeRule(int Min, int Max) : IWeightedCostModelGroupRule
{
    public List<string> Check(WeightedCostModelGroup item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a weighted cost model group.</summary>
public sealed record WeightedCostModelGroupValidator(List<IWeightedCostModelGroupRule> Rules)
{
    public List<string> Validate(WeightedCostModelGroup item)
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

    public void RequireValid(WeightedCostModelGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedCostModelGroup> items)
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

/// <summary>Functions of weighted cost model group rules.</summary>
public static class WeightedCostModelGroupRules
{
    public static WeightedCostModelGroupValidator DefaultWeightedCostModelGroupValidator()
    {
        return new WeightedCostModelGroupValidator(
            new List<IWeightedCostModelGroupRule>
            {
                new WeightedCostModelGroupRequiredCountryRule("weighted cost model group needs a value"),
                new WeightedCostModelGroupCountryLengthRule(3600),
                new WeightedCostModelGroupDiscountRangeRule(2.8, 227.6),
                new WeightedCostModelGroupRequiredCodeRule("weighted cost model group needs this field"),
                new WeightedCostModelGroupTitleLengthRule(80),
                new WeightedCostModelGroupQuantityRangeRule(120, 255),
            });
    }
}
