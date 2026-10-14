namespace Acme.Workforce.Payrollops.Deduction.Cycle;

/// <summary>A rule a deduction preference must pass. Check returns its problems, or none.</summary>
public interface IDeductionPreferenceRule
{
    List<string> Check(DeductionPreference item);
}

public sealed record DeductionPreferenceNameLengthRule(int MaxLength) : IDeductionPreferenceRule
{
    public List<string> Check(DeductionPreference item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionPreferencePhoneLengthRule(int MaxLength) : IDeductionPreferenceRule
{
    public List<string> Check(DeductionPreference item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionPreferenceRequiredGradeRule(string Message) : IDeductionPreferenceRule
{
    public List<string> Check(DeductionPreference item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionPreferenceFloorValueRangeRule(double Min, double Max) : IDeductionPreferenceRule
{
    public List<string> Check(DeductionPreference item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction preference.</summary>
public sealed record DeductionPreferenceValidator(List<IDeductionPreferenceRule> Rules)
{
    public List<string> Validate(DeductionPreference item)
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

    public void RequireValid(DeductionPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionPreference> items)
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

/// <summary>Functions of deduction preference rules.</summary>
public static class DeductionPreferenceRules
{
    public static DeductionPreferenceValidator DefaultDeductionPreferenceValidator()
    {
        return new DeductionPreferenceValidator(
            new List<IDeductionPreferenceRule>
            {
                new DeductionPreferenceNameLengthRule(1000),
                new DeductionPreferencePhoneLengthRule(3600),
                new DeductionPreferenceRequiredGradeRule("deduction preference needs a value"),
                new DeductionPreferenceFloorValueRangeRule(0.1, 3411.1),
            });
    }
}
