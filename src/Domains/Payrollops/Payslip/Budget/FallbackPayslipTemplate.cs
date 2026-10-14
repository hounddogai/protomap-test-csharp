namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback payslip template is in its life cycle.</summary>
public enum FallbackPayslipTemplateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A fallback payslip template. Its code is unique within a tenant.</summary>
public sealed class FallbackPayslipTemplate(string code, string name, double floorValue, int sequence, Dictionary<string, string> attributes, string unit, int endsAt, double rate, FallbackPayslipTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public int Sequence { get; set; } = sequence;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Unit { get; set; } = unit;
    public int EndsAt { get; set; } = endsAt;
    public double Rate { get; set; } = rate;
    public FallbackPayslipTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a fallback payslip template in status requested.</summary>
    public static FallbackPayslipTemplate NewFallbackPayslipTemplate(string code, string name)
    {
        return new FallbackPayslipTemplate(
            code,
            name,
            0.0,
            0,
            new Dictionary<string, string>(),
            "",
            0,
            0.0,
            FallbackPayslipTemplateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPayslipTemplateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPayslipTemplateStatus next)
    {
        switch (Status)
        {
            case FallbackPayslipTemplateStatus.Requested:
                switch (next)
                {
                    case FallbackPayslipTemplateStatus.Revoked:
                        return true;
                }
                return false;
            case FallbackPayslipTemplateStatus.Granted:
                switch (next)
                {
                    case FallbackPayslipTemplateStatus.Revoked:
                        return true;
                }
                return false;
            case FallbackPayslipTemplateStatus.Revoked:
                return next == FallbackPayslipTemplateStatus.Expired;
        }
        return false;
    }

    public void MoveTo(FallbackPayslipTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback payslip template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"fallback payslip template"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedVolume()
    {
        return Math.Round(Rate + (double)Sequence, 2);
    }

    public FallbackPayslipTemplate Copy()
    {
        return new FallbackPayslipTemplate(Code, Name, FloorValue, Sequence, Attributes, Unit, EndsAt, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
