namespace Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>A rule a warranty request must pass. Check returns its problems, or none.</summary>
public interface IWarrantyRequestRule
{
    List<string> Check(WarrantyRequest item);
}

public sealed record WarrantyRequestRequiredCodeRule(string Message) : IWarrantyRequestRule
{
    public List<string> Check(WarrantyRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyRequestTitleLengthRule(int MaxLength) : IWarrantyRequestRule
{
    public List<string> Check(WarrantyRequest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyRequestRequiredChecksumRule(string Message) : IWarrantyRequestRule
{
    public List<string> Check(WarrantyRequest item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty request.</summary>
public sealed record WarrantyRequestValidator(List<IWarrantyRequestRule> Rules)
{
    public List<string> Validate(WarrantyRequest item)
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

    public void RequireValid(WarrantyRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyRequest> items)
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

/// <summary>Functions of warranty request rules.</summary>
public static class WarrantyRequestRules
{
    public static WarrantyRequestValidator DefaultWarrantyRequestValidator()
    {
        return new WarrantyRequestValidator(
            new List<IWarrantyRequestRule>
            {
                new WarrantyRequestRequiredCodeRule("warranty request needs a value"),
                new WarrantyRequestTitleLengthRule(80),
                new WarrantyRequestRequiredChecksumRule("warranty request needs a value"),
            });
    }
}
