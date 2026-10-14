namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback payslip digest is in its life cycle.</summary>
public enum FallbackPayslipDigestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a fallback payslip digest and its current status.</summary>
public sealed class FallbackPayslipDigest(string code, string name, int retries, double ratio, string externalRef, double maximum, string skill, int capacity, int expiresAt, int limit, FallbackPayslipDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public double Ratio { get; set; } = ratio;
    public string ExternalRef { get; set; } = externalRef;
    public double Maximum { get; set; } = maximum;
    public string Skill { get; set; } = skill;
    public int Capacity { get; set; } = capacity;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Limit { get; set; } = limit;
    public FallbackPayslipDigestStatus Status { get; set; } = status;

    /// <summary>Starts a fallback payslip digest in status created.</summary>
    public static FallbackPayslipDigest NewFallbackPayslipDigest(string code, string name)
    {
        return new FallbackPayslipDigest(
            code,
            name,
            0,
            0.0,
            "",
            0.0,
            "",
            0,
            0,
            0,
            FallbackPayslipDigestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPayslipDigestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPayslipDigestStatus next)
    {
        switch (Status)
        {
            case FallbackPayslipDigestStatus.Created:
                return next == FallbackPayslipDigestStatus.Validated;
            case FallbackPayslipDigestStatus.Validated:
                return next == FallbackPayslipDigestStatus.Published || next == FallbackPayslipDigestStatus.Retired;
            case FallbackPayslipDigestStatus.Published:
                switch (next)
                {
                    case FallbackPayslipDigestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackPayslipDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback payslip digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"fallback payslip digest"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)ExpiresAt + Ratio, 2);
    }

    public FallbackPayslipDigest Copy()
    {
        return new FallbackPayslipDigest(
            Code,
            Name,
            Retries,
            Ratio,
            ExternalRef,
            Maximum,
            Skill,
            Capacity,
            ExpiresAt,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
