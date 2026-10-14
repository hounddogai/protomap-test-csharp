namespace Acme.Workforce.Orgdesign.CostModel.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a cost model mapping must pass. Check returns its problems, or none.</summary>
public interface ICostModelMappingRule
{
    List<string> Check(CostModelMapping item);
}

public sealed record CostModelMappingRequiredTimezoneRule(string Message) : ICostModelMappingRule
{
    public List<string> Check(CostModelMapping item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelMappingAllowedTimezoneRule(List<string> Allowed) : ICostModelMappingRule
{
    public List<string> Check(CostModelMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record CostModelMappingCodeLengthRule(int MaxLength) : ICostModelMappingRule
{
    public List<string> Check(CostModelMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelMappingRequiredDepartmentRule(string Message) : ICostModelMappingRule
{
    public List<string> Check(CostModelMapping item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelMappingNameLengthRule(int MaxLength) : ICostModelMappingRule
{
    public List<string> Check(CostModelMapping item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelMappingThresholdRangeRule(double Min, double Max) : ICostModelMappingRule
{
    public List<string> Check(CostModelMapping item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model mapping.</summary>
public sealed record CostModelMappingValidator(List<ICostModelMappingRule> Rules)
{
    public List<string> Validate(CostModelMapping item)
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

    public void RequireValid(CostModelMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelMapping> items)
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

/// <summary>Functions of cost model mapping rules.</summary>
public static class CostModelMappingRules
{
    public static CostModelMappingValidator DefaultCostModelMappingValidator()
    {
        return new CostModelMappingValidator(
            new List<ICostModelMappingRule>
            {
                new CostModelMappingRequiredTimezoneRule("cost model mapping needs a value"),
                new CostModelMappingAllowedTimezoneRule(new List<string>()),
                new CostModelMappingCodeLengthRule(32),
                new CostModelMappingRequiredDepartmentRule("cost model mapping needs a value"),
                new CostModelMappingNameLengthRule(3600),
                new CostModelMappingThresholdRangeRule(1.0, 1922.6),
            });
    }
}
