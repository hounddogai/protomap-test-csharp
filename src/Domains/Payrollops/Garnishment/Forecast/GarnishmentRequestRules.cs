namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>A rule a garnishment request must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentRequestRule
{
    List<string> Check(GarnishmentRequest item);
}

public sealed record GarnishmentRequestFteRangeRule(double Min, double Max) : IGarnishmentRequestRule
{
    public List<string> Check(GarnishmentRequest item)
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

public sealed record GarnishmentRequestRequiredContactEmailRule(string Message) : IGarnishmentRequestRule
{
    public List<string> Check(GarnishmentRequest item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentRequestContactEmailLengthRule(int MaxLength) : IGarnishmentRequestRule
{
    public List<string> Check(GarnishmentRequest item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentRequestRequiredEmployeeIdRule(string Message) : IGarnishmentRequestRule
{
    public List<string> Check(GarnishmentRequest item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentRequestTitleLengthRule(int MaxLength) : IGarnishmentRequestRule
{
    public List<string> Check(GarnishmentRequest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentRequestMaximumRangeRule(double Min, double Max) : IGarnishmentRequestRule
{
    public List<string> Check(GarnishmentRequest item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment request.</summary>
public sealed record GarnishmentRequestValidator(List<IGarnishmentRequestRule> Rules)
{
    public List<string> Validate(GarnishmentRequest item)
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

    public void RequireValid(GarnishmentRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentRequest> items)
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

/// <summary>Functions of garnishment request rules.</summary>
public static class GarnishmentRequestRules
{
    public static GarnishmentRequestValidator DefaultGarnishmentRequestValidator()
    {
        return new GarnishmentRequestValidator(
            new List<IGarnishmentRequestRule>
            {
                new GarnishmentRequestFteRangeRule(2.7, 3918.7),
                new GarnishmentRequestRequiredContactEmailRule("garnishment request needs a value"),
                new GarnishmentRequestContactEmailLengthRule(1000),
                new GarnishmentRequestRequiredEmployeeIdRule("garnishment request needs an entry"),
                new GarnishmentRequestTitleLengthRule(120),
                new GarnishmentRequestMaximumRangeRule(8.3, 2678.7),
            });
    }
}
