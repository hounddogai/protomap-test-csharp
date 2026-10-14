namespace Acme.Workforce.Payrollops.TaxForm.Metric;

/// <summary>A rule a tax form milestone must pass. Check returns its problems, or none.</summary>
public interface ITaxFormMilestoneRule
{
    List<string> Check(TaxFormMilestone item);
}

public sealed record TaxFormMilestoneChannelLengthRule(int MaxLength) : ITaxFormMilestoneRule
{
    public List<string> Check(TaxFormMilestone item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormMilestoneRequiredCodeRule(string Message) : ITaxFormMilestoneRule
{
    public List<string> Check(TaxFormMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormMilestoneRequiredTitleRule(string Message) : ITaxFormMilestoneRule
{
    public List<string> Check(TaxFormMilestone item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormMilestoneVisibleGuardRule(double Limit) : ITaxFormMilestoneRule
{
    public List<string> Check(TaxFormMilestone item)
    {
        if (item.Visible && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormMilestoneCodePrefixRule(string Prefix) : ITaxFormMilestoneRule
{
    public List<string> Check(TaxFormMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form milestone.</summary>
public sealed record TaxFormMilestoneValidator(List<ITaxFormMilestoneRule> Rules)
{
    public List<string> Validate(TaxFormMilestone item)
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

    public void RequireValid(TaxFormMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormMilestone> items)
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

/// <summary>Functions of tax form milestone rules.</summary>
public static class TaxFormMilestoneRules
{
    public static TaxFormMilestoneValidator DefaultTaxFormMilestoneValidator()
    {
        return new TaxFormMilestoneValidator(
            new List<ITaxFormMilestoneRule>
            {
                new TaxFormMilestoneChannelLengthRule(1000),
                new TaxFormMilestoneRequiredCodeRule("tax form milestone needs this field"),
                new TaxFormMilestoneRequiredTitleRule("tax form milestone needs a value"),
                new TaxFormMilestoneVisibleGuardRule(8.5),
                new TaxFormMilestoneCodePrefixRule("ACM"),
            });
    }
}
