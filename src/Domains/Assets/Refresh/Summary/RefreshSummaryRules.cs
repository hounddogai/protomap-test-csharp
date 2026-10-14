namespace Acme.Workforce.Assets.Refresh.Summary;

/// <summary>A rule a refresh summary must pass. Check returns its problems, or none.</summary>
public interface IRefreshSummaryRule
{
    List<string> Check(RefreshSummary item);
}

public sealed record RefreshSummaryLabelLengthRule(int MaxLength) : IRefreshSummaryRule
{
    public List<string> Check(RefreshSummary item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshSummaryContactEmailLengthRule(int MaxLength) : IRefreshSummaryRule
{
    public List<string> Check(RefreshSummary item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshSummaryActiveGuardRule(double Limit) : IRefreshSummaryRule
{
    public List<string> Check(RefreshSummary item)
    {
        if (item.Active && item.Margin > Limit)
        {
            return new List<string> { $"margin {item.Margin} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record RefreshSummaryRequiredSalaryBandRule(string Message) : IRefreshSummaryRule
{
    public List<string> Check(RefreshSummary item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh summary.</summary>
public sealed record RefreshSummaryValidator(List<IRefreshSummaryRule> Rules)
{
    public List<string> Validate(RefreshSummary item)
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

    public void RequireValid(RefreshSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshSummary> items)
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

/// <summary>Functions of refresh summary rules.</summary>
public static class RefreshSummaryRules
{
    public static RefreshSummaryValidator DefaultRefreshSummaryValidator()
    {
        return new RefreshSummaryValidator(
            new List<IRefreshSummaryRule>
            {
                new RefreshSummaryLabelLengthRule(16),
                new RefreshSummaryContactEmailLengthRule(32),
                new RefreshSummaryActiveGuardRule(9.6),
                new RefreshSummaryRequiredSalaryBandRule("refresh summary needs a value"),
            });
    }
}
