namespace Acme.Workforce.Assets.Laptop.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a laptop exception must pass. Check returns its problems, or none.</summary>
public interface ILaptopExceptionRule
{
    List<string> Check(LaptopException item);
}

public sealed record LaptopExceptionCodeLengthRule(int MaxLength) : ILaptopExceptionRule
{
    public List<string> Check(LaptopException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopExceptionRequiredCodeRule(string Message) : ILaptopExceptionRule
{
    public List<string> Check(LaptopException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopExceptionCodePrefixRule(string Prefix) : ILaptopExceptionRule
{
    public List<string> Check(LaptopException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopExceptionRequiredLabelRule(string Message) : ILaptopExceptionRule
{
    public List<string> Check(LaptopException item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopExceptionReviewerLengthRule(int MaxLength) : ILaptopExceptionRule
{
    public List<string> Check(LaptopException item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopExceptionAllowedCodeRule(List<string> Allowed) : ILaptopExceptionRule
{
    public List<string> Check(LaptopException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop exception.</summary>
public sealed record LaptopExceptionValidator(List<ILaptopExceptionRule> Rules)
{
    public List<string> Validate(LaptopException item)
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

    public void RequireValid(LaptopException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopException> items)
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

/// <summary>Functions of laptop exception rules.</summary>
public static class LaptopExceptionRules
{
    public static LaptopExceptionValidator DefaultLaptopExceptionValidator()
    {
        return new LaptopExceptionValidator(
            new List<ILaptopExceptionRule>
            {
                new LaptopExceptionCodeLengthRule(1000),
                new LaptopExceptionRequiredCodeRule("laptop exception needs this field"),
                new LaptopExceptionCodePrefixRule("X"),
                new LaptopExceptionRequiredLabelRule("laptop exception needs this field"),
                new LaptopExceptionReviewerLengthRule(255),
                new LaptopExceptionAllowedCodeRule(new List<string> { "weighted", "batched", "pending" }),
            });
    }
}
