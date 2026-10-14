namespace Acme.Workforce.Absence.Balance.Baseline;

/// <summary>A rule a adaptive balance summary must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveBalanceSummaryRule
{
    List<string> Check(AdaptiveBalanceSummary item);
}

public sealed record AdaptiveBalanceSummaryContactEmailLengthRule(int MaxLength) : IAdaptiveBalanceSummaryRule
{
    public List<string> Check(AdaptiveBalanceSummary item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBalanceSummaryRequiredLabelRule(string Message) : IAdaptiveBalanceSummaryRule
{
    public List<string> Check(AdaptiveBalanceSummary item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBalanceSummaryRequiredContactEmailRule(string Message) : IAdaptiveBalanceSummaryRule
{
    public List<string> Check(AdaptiveBalanceSummary item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive balance summary.</summary>
public sealed record AdaptiveBalanceSummaryValidator(List<IAdaptiveBalanceSummaryRule> Rules)
{
    public List<string> Validate(AdaptiveBalanceSummary item)
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

    public void RequireValid(AdaptiveBalanceSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveBalanceSummary> items)
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

/// <summary>Functions of adaptive balance summary rules.</summary>
public static class AdaptiveBalanceSummaryRules
{
    public static AdaptiveBalanceSummaryValidator DefaultAdaptiveBalanceSummaryValidator()
    {
        return new AdaptiveBalanceSummaryValidator(
            new List<IAdaptiveBalanceSummaryRule>
            {
                new AdaptiveBalanceSummaryContactEmailLengthRule(120),
                new AdaptiveBalanceSummaryRequiredLabelRule("adaptive balance summary needs this field"),
                new AdaptiveBalanceSummaryRequiredContactEmailRule("adaptive balance summary needs this field"),
            });
    }
}
