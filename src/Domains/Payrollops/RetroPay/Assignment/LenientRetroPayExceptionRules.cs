namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>A rule a lenient retro pay exception must pass. Check returns its problems, or none.</summary>
public interface ILenientRetroPayExceptionRule
{
    List<string> Check(LenientRetroPayException item);
}

public sealed record LenientRetroPayExceptionLabelLengthRule(int MaxLength) : ILenientRetroPayExceptionRule
{
    public List<string> Check(LenientRetroPayException item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientRetroPayExceptionBatchSizeRangeRule(int Min, int Max) : ILenientRetroPayExceptionRule
{
    public List<string> Check(LenientRetroPayException item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientRetroPayExceptionRequiredCountryRule(string Message) : ILenientRetroPayExceptionRule
{
    public List<string> Check(LenientRetroPayException item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient retro pay exception.</summary>
public sealed record LenientRetroPayExceptionValidator(List<ILenientRetroPayExceptionRule> Rules)
{
    public List<string> Validate(LenientRetroPayException item)
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

    public void RequireValid(LenientRetroPayException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientRetroPayException> items)
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

/// <summary>Functions of lenient retro pay exception rules.</summary>
public static class LenientRetroPayExceptionRules
{
    public static LenientRetroPayExceptionValidator DefaultLenientRetroPayExceptionValidator()
    {
        return new LenientRetroPayExceptionValidator(
            new List<ILenientRetroPayExceptionRule>
            {
                new LenientRetroPayExceptionLabelLengthRule(86400),
                new LenientRetroPayExceptionBatchSizeRangeRule(255, 1000),
                new LenientRetroPayExceptionRequiredCountryRule("lenient retro pay exception needs this field"),
            });
    }
}
