namespace Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>A rule a accrual bundle must pass. Check returns its problems, or none.</summary>
public interface IAccrualBundleRule
{
    List<string> Check(AccrualBundle item);
}

public sealed record AccrualBundleRequiredCodeRule(string Message) : IAccrualBundleRule
{
    public List<string> Check(AccrualBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualBundleContactEmailLengthRule(int MaxLength) : IAccrualBundleRule
{
    public List<string> Check(AccrualBundle item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualBundleNameLengthRule(int MaxLength) : IAccrualBundleRule
{
    public List<string> Check(AccrualBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualBundleCodePrefixRule(string Prefix) : IAccrualBundleRule
{
    public List<string> Check(AccrualBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual bundle.</summary>
public sealed record AccrualBundleValidator(List<IAccrualBundleRule> Rules)
{
    public List<string> Validate(AccrualBundle item)
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

    public void RequireValid(AccrualBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualBundle> items)
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

/// <summary>Functions of accrual bundle rules.</summary>
public static class AccrualBundleRules
{
    public static AccrualBundleValidator DefaultAccrualBundleValidator()
    {
        return new AccrualBundleValidator(
            new List<IAccrualBundleRule>
            {
                new AccrualBundleRequiredCodeRule("accrual bundle needs a value"),
                new AccrualBundleContactEmailLengthRule(255),
                new AccrualBundleNameLengthRule(86400),
                new AccrualBundleCodePrefixRule("REF"),
            });
    }
}
