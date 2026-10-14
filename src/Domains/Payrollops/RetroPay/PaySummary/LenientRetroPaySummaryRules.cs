namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>A rule a lenient retro pay summary must pass. Check returns its problems, or none.</summary>
public interface ILenientRetroPaySummaryRule
{
    List<string> Check(LenientRetroPaySummary item);
}

public sealed record LenientRetroPaySummaryFteRangeRule(double Min, double Max) : ILenientRetroPaySummaryRule
{
    public List<string> Check(LenientRetroPaySummary item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientRetroPaySummaryRequiredSummaryRule(string Message) : ILenientRetroPaySummaryRule
{
    public List<string> Check(LenientRetroPaySummary item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientRetroPaySummaryRequiredCodeRule(string Message) : ILenientRetroPaySummaryRule
{
    public List<string> Check(LenientRetroPaySummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientRetroPaySummaryCountryLengthRule(int MaxLength) : ILenientRetroPaySummaryRule
{
    public List<string> Check(LenientRetroPaySummary item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient retro pay summary.</summary>
public sealed record LenientRetroPaySummaryValidator(List<ILenientRetroPaySummaryRule> Rules)
{
    public List<string> Validate(LenientRetroPaySummary item)
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

    public void RequireValid(LenientRetroPaySummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientRetroPaySummary> items)
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

/// <summary>Functions of lenient retro pay summary rules.</summary>
public static class LenientRetroPaySummaryRules
{
    public static LenientRetroPaySummaryValidator DefaultLenientRetroPaySummaryValidator()
    {
        return new LenientRetroPaySummaryValidator(
            new List<ILenientRetroPaySummaryRule>
            {
                new LenientRetroPaySummaryFteRangeRule(6.5, 902.1),
                new LenientRetroPaySummaryRequiredSummaryRule("lenient retro pay summary needs a value"),
                new LenientRetroPaySummaryRequiredCodeRule("lenient retro pay summary needs an entry"),
                new LenientRetroPaySummaryCountryLengthRule(80),
            });
    }
}
