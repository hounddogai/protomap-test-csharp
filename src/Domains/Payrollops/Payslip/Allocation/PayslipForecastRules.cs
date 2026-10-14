namespace Acme.Workforce.Payrollops.Payslip.Allocation;

/// <summary>A rule a payslip forecast must pass. Check returns its problems, or none.</summary>
public interface IPayslipForecastRule
{
    List<string> Check(PayslipForecast item);
}

public sealed record PayslipForecastWindowDaysRangeRule(int Min, int Max) : IPayslipForecastRule
{
    public List<string> Check(PayslipForecast item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipForecastRequiredCodeRule(string Message) : IPayslipForecastRule
{
    public List<string> Check(PayslipForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipForecastRequiredNameRule(string Message) : IPayslipForecastRule
{
    public List<string> Check(PayslipForecast item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipForecastCodePrefixRule(string Prefix) : IPayslipForecastRule
{
    public List<string> Check(PayslipForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipForecastRequiredLabelRule(string Message) : IPayslipForecastRule
{
    public List<string> Check(PayslipForecast item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip forecast.</summary>
public sealed record PayslipForecastValidator(List<IPayslipForecastRule> Rules)
{
    public List<string> Validate(PayslipForecast item)
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

    public void RequireValid(PayslipForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipForecast> items)
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

/// <summary>Functions of payslip forecast rules.</summary>
public static class PayslipForecastRules
{
    public static PayslipForecastValidator DefaultPayslipForecastValidator()
    {
        return new PayslipForecastValidator(
            new List<IPayslipForecastRule>
            {
                new PayslipForecastWindowDaysRangeRule(1000, 3600),
                new PayslipForecastRequiredCodeRule("payslip forecast needs a value"),
                new PayslipForecastRequiredNameRule("payslip forecast needs this field"),
                new PayslipForecastCodePrefixRule("REF"),
                new PayslipForecastRequiredLabelRule("payslip forecast needs an entry"),
            });
    }
}
