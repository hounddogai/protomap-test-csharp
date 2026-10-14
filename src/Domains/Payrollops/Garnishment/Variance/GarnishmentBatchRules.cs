namespace Acme.Workforce.Payrollops.Garnishment.Variance;

/// <summary>A rule a garnishment batch must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentBatchRule
{
    List<string> Check(GarnishmentBatch item);
}

public sealed record GarnishmentBatchGradeLengthRule(int MaxLength) : IGarnishmentBatchRule
{
    public List<string> Check(GarnishmentBatch item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentBatchRequiredCodeRule(string Message) : IGarnishmentBatchRule
{
    public List<string> Check(GarnishmentBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentBatchCodePrefixRule(string Prefix) : IGarnishmentBatchRule
{
    public List<string> Check(GarnishmentBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentBatchQuantityRangeRule(int Min, int Max) : IGarnishmentBatchRule
{
    public List<string> Check(GarnishmentBatch item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentBatchTitleLengthRule(int MaxLength) : IGarnishmentBatchRule
{
    public List<string> Check(GarnishmentBatch item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment batch.</summary>
public sealed record GarnishmentBatchValidator(List<IGarnishmentBatchRule> Rules)
{
    public List<string> Validate(GarnishmentBatch item)
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

    public void RequireValid(GarnishmentBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentBatch> items)
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

/// <summary>Functions of garnishment batch rules.</summary>
public static class GarnishmentBatchRules
{
    public static GarnishmentBatchValidator DefaultGarnishmentBatchValidator()
    {
        return new GarnishmentBatchValidator(
            new List<IGarnishmentBatchRule>
            {
                new GarnishmentBatchGradeLengthRule(86400),
                new GarnishmentBatchRequiredCodeRule("garnishment batch needs an entry"),
                new GarnishmentBatchCodePrefixRule("REF"),
                new GarnishmentBatchQuantityRangeRule(3600, 3600),
                new GarnishmentBatchTitleLengthRule(120),
            });
    }
}
