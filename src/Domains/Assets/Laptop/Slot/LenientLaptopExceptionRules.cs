namespace Acme.Workforce.Assets.Laptop.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient laptop exception must pass. Check returns its problems, or none.</summary>
public interface ILenientLaptopExceptionRule
{
    List<string> Check(LenientLaptopException item);
}

public sealed record LenientLaptopExceptionRequiredNameRule(string Message) : ILenientLaptopExceptionRule
{
    public List<string> Check(LenientLaptopException item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientLaptopExceptionAllowedSourceRule(List<string> Allowed) : ILenientLaptopExceptionRule
{
    public List<string> Check(LenientLaptopException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is stale" };
        }
        return new List<string>();
    }
}

public sealed record LenientLaptopExceptionCreatedAtRangeRule(int Min, int Max) : ILenientLaptopExceptionRule
{
    public List<string> Check(LenientLaptopException item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLaptopExceptionSourceLengthRule(int MaxLength) : ILenientLaptopExceptionRule
{
    public List<string> Check(LenientLaptopException item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient laptop exception.</summary>
public sealed record LenientLaptopExceptionValidator(List<ILenientLaptopExceptionRule> Rules)
{
    public List<string> Validate(LenientLaptopException item)
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

    public void RequireValid(LenientLaptopException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientLaptopException> items)
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

/// <summary>Functions of lenient laptop exception rules.</summary>
public static class LenientLaptopExceptionRules
{
    public static LenientLaptopExceptionValidator DefaultLenientLaptopExceptionValidator()
    {
        return new LenientLaptopExceptionValidator(
            new List<ILenientLaptopExceptionRule>
            {
                new LenientLaptopExceptionRequiredNameRule("lenient laptop exception needs this field"),
                new LenientLaptopExceptionAllowedSourceRule(new List<string> { "primary", "default", "tiered" }),
                new LenientLaptopExceptionCreatedAtRangeRule(255, 3600),
                new LenientLaptopExceptionSourceLengthRule(80),
            });
    }
}
