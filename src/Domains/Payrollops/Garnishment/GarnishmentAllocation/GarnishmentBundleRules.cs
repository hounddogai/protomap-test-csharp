namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment bundle must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentBundleRule
{
    List<string> Check(GarnishmentBundle item);
}

public sealed record GarnishmentBundleRequiredTitleRule(string Message) : IGarnishmentBundleRule
{
    public List<string> Check(GarnishmentBundle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentBundleOwnerIdLengthRule(int MaxLength) : IGarnishmentBundleRule
{
    public List<string> Check(GarnishmentBundle item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentBundleWeightRangeRule(double Min, double Max) : IGarnishmentBundleRule
{
    public List<string> Check(GarnishmentBundle item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentBundleAllowedTitleRule(List<string> Allowed) : IGarnishmentBundleRule
{
    public List<string> Check(GarnishmentBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentBundleCodeLengthRule(int MaxLength) : IGarnishmentBundleRule
{
    public List<string> Check(GarnishmentBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment bundle.</summary>
public sealed record GarnishmentBundleValidator(List<IGarnishmentBundleRule> Rules)
{
    public List<string> Validate(GarnishmentBundle item)
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

    public void RequireValid(GarnishmentBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentBundle> items)
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

/// <summary>Functions of garnishment bundle rules.</summary>
public static class GarnishmentBundleRules
{
    public static GarnishmentBundleValidator DefaultGarnishmentBundleValidator()
    {
        return new GarnishmentBundleValidator(
            new List<IGarnishmentBundleRule>
            {
                new GarnishmentBundleRequiredTitleRule("garnishment bundle needs this field"),
                new GarnishmentBundleOwnerIdLengthRule(3600),
                new GarnishmentBundleWeightRangeRule(4.7, 663.2),
                new GarnishmentBundleAllowedTitleRule(new List<string> { "seasonal", "primary" }),
                new GarnishmentBundleCodeLengthRule(3600),
            });
    }
}
