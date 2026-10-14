namespace Acme.Workforce.Orgdesign.CostModel.Snapshot;

/// <summary>A rule a cost model manifest must pass. Check returns its problems, or none.</summary>
public interface ICostModelManifestRule
{
    List<string> Check(CostModelManifest item);
}

public sealed record CostModelManifestRequiredUnitRule(string Message) : ICostModelManifestRule
{
    public List<string> Check(CostModelManifest item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelManifestRequiredNameRule(string Message) : ICostModelManifestRule
{
    public List<string> Check(CostModelManifest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelManifestQuantityRangeRule(int Min, int Max) : ICostModelManifestRule
{
    public List<string> Check(CostModelManifest item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelManifestCodeLengthRule(int MaxLength) : ICostModelManifestRule
{
    public List<string> Check(CostModelManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelManifestExpiresAtRangeRule(int Min, int Max) : ICostModelManifestRule
{
    public List<string> Check(CostModelManifest item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model manifest.</summary>
public sealed record CostModelManifestValidator(List<ICostModelManifestRule> Rules)
{
    public List<string> Validate(CostModelManifest item)
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

    public void RequireValid(CostModelManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelManifest> items)
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

/// <summary>Functions of cost model manifest rules.</summary>
public static class CostModelManifestRules
{
    public static CostModelManifestValidator DefaultCostModelManifestValidator()
    {
        return new CostModelManifestValidator(
            new List<ICostModelManifestRule>
            {
                new CostModelManifestRequiredUnitRule("cost model manifest needs an entry"),
                new CostModelManifestRequiredNameRule("cost model manifest needs an entry"),
                new CostModelManifestQuantityRangeRule(16, 64),
                new CostModelManifestCodeLengthRule(86400),
                new CostModelManifestExpiresAtRangeRule(255, 86400),
            });
    }
}
