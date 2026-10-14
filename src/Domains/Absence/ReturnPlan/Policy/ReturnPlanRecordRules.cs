namespace Acme.Workforce.Absence.ReturnPlan.Policy;

/// <summary>A rule a return plan record must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanRecordRule
{
    List<string> Check(ReturnPlanRecord item);
}

public sealed record ReturnPlanRecordRequiredLabelRule(string Message) : IReturnPlanRecordRule
{
    public List<string> Check(ReturnPlanRecord item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanRecordAttemptsRangeRule(int Min, int Max) : IReturnPlanRecordRule
{
    public List<string> Check(ReturnPlanRecord item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanRecordCodePrefixRule(string Prefix) : IReturnPlanRecordRule
{
    public List<string> Check(ReturnPlanRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanRecordLabelLengthRule(int MaxLength) : IReturnPlanRecordRule
{
    public List<string> Check(ReturnPlanRecord item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanRecordWindowDaysRangeRule(int Min, int Max) : IReturnPlanRecordRule
{
    public List<string> Check(ReturnPlanRecord item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan record.</summary>
public sealed record ReturnPlanRecordValidator(List<IReturnPlanRecordRule> Rules)
{
    public List<string> Validate(ReturnPlanRecord item)
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

    public void RequireValid(ReturnPlanRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanRecord> items)
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

/// <summary>Functions of return plan record rules.</summary>
public static class ReturnPlanRecordRules
{
    public static ReturnPlanRecordValidator DefaultReturnPlanRecordValidator()
    {
        return new ReturnPlanRecordValidator(
            new List<IReturnPlanRecordRule>
            {
                new ReturnPlanRecordRequiredLabelRule("return plan record needs a value"),
                new ReturnPlanRecordAttemptsRangeRule(16, 16),
                new ReturnPlanRecordCodePrefixRule("REF"),
                new ReturnPlanRecordLabelLengthRule(120),
                new ReturnPlanRecordWindowDaysRangeRule(1000, 1000),
            });
    }
}
