namespace Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>A rule a manual position summary must pass. Check returns its problems, or none.</summary>
public interface IManualPositionSummaryRule
{
    List<string> Check(ManualPositionSummary item);
}

public sealed record ManualPositionSummaryRequiredTitleRule(string Message) : IManualPositionSummaryRule
{
    public List<string> Check(ManualPositionSummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualPositionSummaryRequiredCodeRule(string Message) : IManualPositionSummaryRule
{
    public List<string> Check(ManualPositionSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualPositionSummaryCodeLengthRule(int MaxLength) : IManualPositionSummaryRule
{
    public List<string> Check(ManualPositionSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualPositionSummaryRequiredTeamRule(string Message) : IManualPositionSummaryRule
{
    public List<string> Check(ManualPositionSummary item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualPositionSummaryRequiredContactEmailRule(string Message) : IManualPositionSummaryRule
{
    public List<string> Check(ManualPositionSummary item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual position summary.</summary>
public sealed record ManualPositionSummaryValidator(List<IManualPositionSummaryRule> Rules)
{
    public List<string> Validate(ManualPositionSummary item)
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

    public void RequireValid(ManualPositionSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualPositionSummary> items)
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

/// <summary>Functions of manual position summary rules.</summary>
public static class ManualPositionSummaryRules
{
    public static ManualPositionSummaryValidator DefaultManualPositionSummaryValidator()
    {
        return new ManualPositionSummaryValidator(
            new List<IManualPositionSummaryRule>
            {
                new ManualPositionSummaryRequiredTitleRule("manual position summary needs an entry"),
                new ManualPositionSummaryRequiredCodeRule("manual position summary needs this field"),
                new ManualPositionSummaryCodeLengthRule(64),
                new ManualPositionSummaryRequiredTeamRule("manual position summary needs this field"),
                new ManualPositionSummaryRequiredContactEmailRule("manual position summary needs a value"),
            });
    }
}
