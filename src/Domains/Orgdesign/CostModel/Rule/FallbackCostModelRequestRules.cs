namespace Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>A rule a fallback cost model request must pass. Check returns its problems, or none.</summary>
public interface IFallbackCostModelRequestRule
{
    List<string> Check(FallbackCostModelRequest item);
}

public sealed record FallbackCostModelRequestContactEmailLengthRule(int MaxLength) : IFallbackCostModelRequestRule
{
    public List<string> Check(FallbackCostModelRequest item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackCostModelRequestRequiredContactEmailRule(string Message) : IFallbackCostModelRequestRule
{
    public List<string> Check(FallbackCostModelRequest item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackCostModelRequestRequiredLabelRule(string Message) : IFallbackCostModelRequestRule
{
    public List<string> Check(FallbackCostModelRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback cost model request.</summary>
public sealed record FallbackCostModelRequestValidator(List<IFallbackCostModelRequestRule> Rules)
{
    public List<string> Validate(FallbackCostModelRequest item)
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

    public void RequireValid(FallbackCostModelRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackCostModelRequest> items)
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

/// <summary>Functions of fallback cost model request rules.</summary>
public static class FallbackCostModelRequestRules
{
    public static FallbackCostModelRequestValidator DefaultFallbackCostModelRequestValidator()
    {
        return new FallbackCostModelRequestValidator(
            new List<IFallbackCostModelRequestRule>
            {
                new FallbackCostModelRequestContactEmailLengthRule(3600),
                new FallbackCostModelRequestRequiredContactEmailRule("fallback cost model request needs an entry"),
                new FallbackCostModelRequestRequiredLabelRule("fallback cost model request needs this field"),
            });
    }
}
