namespace Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>A rule a cost model group must pass. Check returns its problems, or none.</summary>
public interface ICostModelGroupRule
{
    List<string> Check(CostModelGroup item);
}

public sealed record CostModelGroupNameLengthRule(int MaxLength) : ICostModelGroupRule
{
    public List<string> Check(CostModelGroup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelGroupTenureMonthsRangeRule(int Min, int Max) : ICostModelGroupRule
{
    public List<string> Check(CostModelGroup item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CostModelGroupRequiredCodeRule(string Message) : ICostModelGroupRule
{
    public List<string> Check(CostModelGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelGroupCodePrefixRule(string Prefix) : ICostModelGroupRule
{
    public List<string> Check(CostModelGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model group.</summary>
public sealed record CostModelGroupValidator(List<ICostModelGroupRule> Rules)
{
    public List<string> Validate(CostModelGroup item)
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

    public void RequireValid(CostModelGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelGroup> items)
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

/// <summary>Functions of cost model group rules.</summary>
public static class CostModelGroupRules
{
    public static CostModelGroupValidator DefaultCostModelGroupValidator()
    {
        return new CostModelGroupValidator(
            new List<ICostModelGroupRule>
            {
                new CostModelGroupNameLengthRule(64),
                new CostModelGroupTenureMonthsRangeRule(64, 86400),
                new CostModelGroupRequiredCodeRule("cost model group needs a value"),
                new CostModelGroupCodePrefixRule("ACM"),
            });
    }
}
