namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>A rule a default exception allocation must pass. Check returns its problems, or none.</summary>
public interface IDefaultExceptionAllocationRule
{
    List<string> Check(DefaultExceptionAllocation item);
}

public sealed record DefaultExceptionAllocationVersionRangeRule(int Min, int Max) : IDefaultExceptionAllocationRule
{
    public List<string> Check(DefaultExceptionAllocation item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultExceptionAllocationCodeLengthRule(int MaxLength) : IDefaultExceptionAllocationRule
{
    public List<string> Check(DefaultExceptionAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultExceptionAllocationCodePrefixRule(string Prefix) : IDefaultExceptionAllocationRule
{
    public List<string> Check(DefaultExceptionAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultExceptionAllocationNameLengthRule(int MaxLength) : IDefaultExceptionAllocationRule
{
    public List<string> Check(DefaultExceptionAllocation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultExceptionAllocationRequiredCodeRule(string Message) : IDefaultExceptionAllocationRule
{
    public List<string> Check(DefaultExceptionAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default exception allocation.</summary>
public sealed record DefaultExceptionAllocationValidator(List<IDefaultExceptionAllocationRule> Rules)
{
    public List<string> Validate(DefaultExceptionAllocation item)
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

    public void RequireValid(DefaultExceptionAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultExceptionAllocation> items)
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

/// <summary>Functions of default exception allocation rules.</summary>
public static class DefaultExceptionAllocationRules
{
    public static DefaultExceptionAllocationValidator DefaultDefaultExceptionAllocationValidator()
    {
        return new DefaultExceptionAllocationValidator(
            new List<IDefaultExceptionAllocationRule>
            {
                new DefaultExceptionAllocationVersionRangeRule(80, 255),
                new DefaultExceptionAllocationCodeLengthRule(86400),
                new DefaultExceptionAllocationCodePrefixRule("OPS"),
                new DefaultExceptionAllocationNameLengthRule(120),
                new DefaultExceptionAllocationRequiredCodeRule("default exception allocation needs a value"),
            });
    }
}
