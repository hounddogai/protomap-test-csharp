namespace Acme.Workforce.Orgdesign.CostModel.Policy;

/// <summary>A rule a cost model exception must pass. Check returns its problems, or none.</summary>
public interface ICostModelExceptionRule
{
    List<string> Check(CostModelException item);
}

public sealed record CostModelExceptionVersionRangeRule(int Min, int Max) : ICostModelExceptionRule
{
    public List<string> Check(CostModelException item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelExceptionApprovedGuardRule(int Limit) : ICostModelExceptionRule
{
    public List<string> Check(CostModelException item)
    {
        if (item.Approved && item.Version > Limit)
        {
            return new List<string> { $"version {item.Version} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record CostModelExceptionRequiredNameRule(string Message) : ICostModelExceptionRule
{
    public List<string> Check(CostModelException item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model exception.</summary>
public sealed record CostModelExceptionValidator(List<ICostModelExceptionRule> Rules)
{
    public List<string> Validate(CostModelException item)
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

    public void RequireValid(CostModelException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelException> items)
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

/// <summary>Functions of cost model exception rules.</summary>
public static class CostModelExceptionRules
{
    public static CostModelExceptionValidator DefaultCostModelExceptionValidator()
    {
        return new CostModelExceptionValidator(
            new List<ICostModelExceptionRule>
            {
                new CostModelExceptionVersionRangeRule(32, 1000),
                new CostModelExceptionApprovedGuardRule(80),
                new CostModelExceptionRequiredNameRule("cost model exception needs a value"),
            });
    }
}
