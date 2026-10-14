namespace Acme.Workforce.Engagement.TownHall.Target;

/// <summary>A rule a legacy town hall summary must pass. Check returns its problems, or none.</summary>
public interface ILegacyTownHallSummaryRule
{
    List<string> Check(LegacyTownHallSummary item);
}

public sealed record LegacyTownHallSummaryRequiredCodeRule(string Message) : ILegacyTownHallSummaryRule
{
    public List<string> Check(LegacyTownHallSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallSummaryRequiredJobCodeRule(string Message) : ILegacyTownHallSummaryRule
{
    public List<string> Check(LegacyTownHallSummary item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallSummaryAmountRangeRule(double Min, double Max) : ILegacyTownHallSummaryRule
{
    public List<string> Check(LegacyTownHallSummary item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy town hall summary.</summary>
public sealed record LegacyTownHallSummaryValidator(List<ILegacyTownHallSummaryRule> Rules)
{
    public List<string> Validate(LegacyTownHallSummary item)
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

    public void RequireValid(LegacyTownHallSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTownHallSummary> items)
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

/// <summary>Functions of legacy town hall summary rules.</summary>
public static class LegacyTownHallSummaryRules
{
    public static LegacyTownHallSummaryValidator DefaultLegacyTownHallSummaryValidator()
    {
        return new LegacyTownHallSummaryValidator(
            new List<ILegacyTownHallSummaryRule>
            {
                new LegacyTownHallSummaryRequiredCodeRule("legacy town hall summary needs this field"),
                new LegacyTownHallSummaryRequiredJobCodeRule("legacy town hall summary needs this field"),
                new LegacyTownHallSummaryAmountRangeRule(4.5, 2127.3),
            });
    }
}
