namespace Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>A rule a pay band manifest must pass. Check returns its problems, or none.</summary>
public interface IPayBandManifestRule
{
    List<string> Check(PayBandManifest item);
}

public sealed record PayBandManifestCategoryLengthRule(int MaxLength) : IPayBandManifestRule
{
    public List<string> Check(PayBandManifest item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandManifestRequiredCategoryRule(string Message) : IPayBandManifestRule
{
    public List<string> Check(PayBandManifest item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandManifestRequiredTitleRule(string Message) : IPayBandManifestRule
{
    public List<string> Check(PayBandManifest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandManifestRequiredPhoneRule(string Message) : IPayBandManifestRule
{
    public List<string> Check(PayBandManifest item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band manifest.</summary>
public sealed record PayBandManifestValidator(List<IPayBandManifestRule> Rules)
{
    public List<string> Validate(PayBandManifest item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(PayBandManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandManifest> items)
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

/// <summary>Functions of pay band manifest rules.</summary>
public static class PayBandManifestRules
{
    public static PayBandManifestValidator DefaultPayBandManifestValidator()
    {
        return new PayBandManifestValidator(
            new List<IPayBandManifestRule>
            {
                new PayBandManifestCategoryLengthRule(86400),
                new PayBandManifestRequiredCategoryRule("pay band manifest needs a value"),
                new PayBandManifestRequiredTitleRule("pay band manifest needs an entry"),
                new PayBandManifestRequiredPhoneRule("pay band manifest needs a value"),
            });
    }
}
