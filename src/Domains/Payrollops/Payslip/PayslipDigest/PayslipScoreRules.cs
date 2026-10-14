namespace Acme.Workforce.Payrollops.Payslip.PayslipDigest;

/// <summary>A rule a payslip score must pass. Check returns its problems, or none.</summary>
public interface IPayslipScoreRule
{
    List<string> Check(PayslipScore item);
}

public sealed record PayslipScoreRequiredNameRule(string Message) : IPayslipScoreRule
{
    public List<string> Check(PayslipScore item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipScoreCodePrefixRule(string Prefix) : IPayslipScoreRule
{
    public List<string> Check(PayslipScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipScoreSequenceRangeRule(int Min, int Max) : IPayslipScoreRule
{
    public List<string> Check(PayslipScore item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipScoreRatingRangeRule(double Min, double Max) : IPayslipScoreRule
{
    public List<string> Check(PayslipScore item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipScoreLabelLengthRule(int MaxLength) : IPayslipScoreRule
{
    public List<string> Check(PayslipScore item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip score.</summary>
public sealed record PayslipScoreValidator(List<IPayslipScoreRule> Rules)
{
    public List<string> Validate(PayslipScore item)
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

    public void RequireValid(PayslipScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipScore> items)
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

/// <summary>Functions of payslip score rules.</summary>
public static class PayslipScoreRules
{
    public static PayslipScoreValidator DefaultPayslipScoreValidator()
    {
        return new PayslipScoreValidator(
            new List<IPayslipScoreRule>
            {
                new PayslipScoreRequiredNameRule("payslip score needs this field"),
                new PayslipScoreCodePrefixRule("REF"),
                new PayslipScoreSequenceRangeRule(120, 3600),
                new PayslipScoreRatingRangeRule(9.7, 4968.7),
                new PayslipScoreLabelLengthRule(1000),
            });
    }
}
