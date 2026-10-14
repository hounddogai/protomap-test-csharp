namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling payslip digest is in its life cycle.</summary>
public enum RollingPayslipDigestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a rolling payslip digest and its current status.</summary>
public sealed class RollingPayslipDigest(string code, string label, string badge, int version, string name, double maximum, string ownerId, bool visible, string jobCode, string channel, RollingPayslipDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public int Version { get; set; } = version;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public string OwnerId { get; set; } = ownerId;
    public bool Visible { get; set; } = visible;
    public string JobCode { get; set; } = jobCode;
    public string Channel { get; set; } = channel;
    public RollingPayslipDigestStatus Status { get; set; } = status;

    /// <summary>Starts a rolling payslip digest in status requested.</summary>
    public static RollingPayslipDigest NewRollingPayslipDigest(string code, string label)
    {
        return new RollingPayslipDigest(
            code,
            label,
            "",
            0,
            "",
            0.0,
            "",
            false,
            "",
            "",
            RollingPayslipDigestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPayslipDigestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPayslipDigestStatus next)
    {
        switch (Status)
        {
            case RollingPayslipDigestStatus.Requested:
                switch (next)
                {
                    case RollingPayslipDigestStatus.Granted:
                    case RollingPayslipDigestStatus.Expired:
                        return true;
                }
                return false;
            case RollingPayslipDigestStatus.Granted:
                switch (next)
                {
                    case RollingPayslipDigestStatus.Revoked:
                        return true;
                }
                return false;
            case RollingPayslipDigestStatus.Revoked:
                return next == RollingPayslipDigestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RollingPayslipDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling payslip digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"rolling payslip digest"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Maximum * (double)Version, 2);
    }

    public RollingPayslipDigest Copy()
    {
        return new RollingPayslipDigest(
            Code,
            Label,
            Badge,
            Version,
            Name,
            Maximum,
            OwnerId,
            Visible,
            JobCode,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
