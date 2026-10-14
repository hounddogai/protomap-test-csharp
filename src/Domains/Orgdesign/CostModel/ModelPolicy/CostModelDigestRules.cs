namespace Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

/// <summary>A rule a cost model digest must pass. Check returns its problems, or none.</summary>
public interface ICostModelDigestRule
{
    List<string> Check(CostModelDigest item);
}

public sealed record CostModelDigestDiscountRangeRule(double Min, double Max) : ICostModelDigestRule
{
    public List<string> Check(CostModelDigest item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CostModelDigestAmountRangeRule(double Min, double Max) : ICostModelDigestRule
{
    public List<string> Check(CostModelDigest item)
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

public sealed record CostModelDigestCodePrefixRule(string Prefix) : ICostModelDigestRule
{
    public List<string> Check(CostModelDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelDigestDescriptionLengthRule(int MaxLength) : ICostModelDigestRule
{
    public List<string> Check(CostModelDigest item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelDigestCodeLengthRule(int MaxLength) : ICostModelDigestRule
{
    public List<string> Check(CostModelDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model digest.</summary>
public sealed record CostModelDigestValidator(List<ICostModelDigestRule> Rules)
{
    public List<string> Validate(CostModelDigest item)
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

    public void RequireValid(CostModelDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelDigest> items)
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

/// <summary>Functions of cost model digest rules.</summary>
public static class CostModelDigestRules
{
    public static CostModelDigestValidator DefaultCostModelDigestValidator()
    {
        return new CostModelDigestValidator(
            new List<ICostModelDigestRule>
            {
                new CostModelDigestDiscountRangeRule(7.5, 3609.1),
                new CostModelDigestAmountRangeRule(5.2, 1157.7),
                new CostModelDigestCodePrefixRule("X"),
                new CostModelDigestDescriptionLengthRule(16),
                new CostModelDigestCodeLengthRule(3600),
            });
    }
}
