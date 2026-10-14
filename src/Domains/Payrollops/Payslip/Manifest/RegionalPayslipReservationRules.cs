namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional payslip reservation must pass. Check returns its problems, or none.</summary>
public interface IRegionalPayslipReservationRule
{
    List<string> Check(RegionalPayslipReservation item);
}

public sealed record RegionalPayslipReservationLabelLengthRule(int MaxLength) : IRegionalPayslipReservationRule
{
    public List<string> Check(RegionalPayslipReservation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayslipReservationAllowedCodeRule(List<string> Allowed) : IRegionalPayslipReservationRule
{
    public List<string> Check(RegionalPayslipReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayslipReservationFloorValueRangeRule(double Min, double Max) : IRegionalPayslipReservationRule
{
    public List<string> Check(RegionalPayslipReservation item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayslipReservationLimitRangeRule(int Min, int Max) : IRegionalPayslipReservationRule
{
    public List<string> Check(RegionalPayslipReservation item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional payslip reservation.</summary>
public sealed record RegionalPayslipReservationValidator(List<IRegionalPayslipReservationRule> Rules)
{
    public List<string> Validate(RegionalPayslipReservation item)
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

    public void RequireValid(RegionalPayslipReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalPayslipReservation> items)
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

/// <summary>Functions of regional payslip reservation rules.</summary>
public static class RegionalPayslipReservationRules
{
    public static RegionalPayslipReservationValidator DefaultRegionalPayslipReservationValidator()
    {
        return new RegionalPayslipReservationValidator(
            new List<IRegionalPayslipReservationRule>
            {
                new RegionalPayslipReservationLabelLengthRule(3600),
                new RegionalPayslipReservationAllowedCodeRule(new List<string>()),
                new RegionalPayslipReservationFloorValueRangeRule(3.3, 1028.3),
                new RegionalPayslipReservationLimitRangeRule(255, 255),
            });
    }
}
