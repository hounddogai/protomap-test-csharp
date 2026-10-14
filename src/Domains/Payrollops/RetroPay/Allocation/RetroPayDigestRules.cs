namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>A rule a retro pay digest must pass. Check returns its problems, or none.</summary>
public interface IRetroPayDigestRule
{
    List<string> Check(RetroPayDigest item);
}

public sealed record RetroPayDigestRequiredTitleRule(string Message) : IRetroPayDigestRule
{
    public List<string> Check(RetroPayDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayDigestRequiredCodeRule(string Message) : IRetroPayDigestRule
{
    public List<string> Check(RetroPayDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayDigestSalaryBandLengthRule(int MaxLength) : IRetroPayDigestRule
{
    public List<string> Check(RetroPayDigest item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayDigestVisibleGuardRule(int Limit) : IRetroPayDigestRule
{
    public List<string> Check(RetroPayDigest item)
    {
        if (item.Visible && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayDigestCategoryLengthRule(int MaxLength) : IRetroPayDigestRule
{
    public List<string> Check(RetroPayDigest item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay digest.</summary>
public sealed record RetroPayDigestValidator(List<IRetroPayDigestRule> Rules)
{
    public List<string> Validate(RetroPayDigest item)
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

    public void RequireValid(RetroPayDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayDigest> items)
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

/// <summary>Functions of retro pay digest rules.</summary>
public static class RetroPayDigestRules
{
    public static RetroPayDigestValidator DefaultRetroPayDigestValidator()
    {
        return new RetroPayDigestValidator(
            new List<IRetroPayDigestRule>
            {
                new RetroPayDigestRequiredTitleRule("retro pay digest needs this field"),
                new RetroPayDigestRequiredCodeRule("retro pay digest needs a value"),
                new RetroPayDigestSalaryBandLengthRule(255),
                new RetroPayDigestVisibleGuardRule(1000),
                new RetroPayDigestCategoryLengthRule(80),
            });
    }
}
