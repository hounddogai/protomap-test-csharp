namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient pay band bundle is in its life cycle.</summary>
public enum LenientPayBandBundleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A lenient pay band bundle as the pay band team keeps it.</summary>
public sealed class LenientPayBandBundle(string code, string label, int expiresAt, string jobCode, int revision, string employeeId, string category, string phone, int version, string name, LenientPayBandBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public string JobCode { get; set; } = jobCode;
    public int Revision { get; set; } = revision;
    public string EmployeeId { get; set; } = employeeId;
    public string Category { get; set; } = category;
    public string Phone { get; set; } = phone;
    public int Version { get; set; } = version;
    public string Name { get; set; } = name;
    public LenientPayBandBundleStatus Status { get; set; } = status;

    /// <summary>Starts a lenient pay band bundle in status requested.</summary>
    public static LenientPayBandBundle NewLenientPayBandBundle(string code, string label)
    {
        return new LenientPayBandBundle(
            code,
            label,
            0,
            "",
            0,
            "",
            "",
            "",
            0,
            "",
            LenientPayBandBundleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPayBandBundleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPayBandBundleStatus next)
    {
        switch (Status)
        {
            case LenientPayBandBundleStatus.Requested:
                return next == LenientPayBandBundleStatus.Granted || next == LenientPayBandBundleStatus.Expired;
            case LenientPayBandBundleStatus.Granted:
                switch (next)
                {
                    case LenientPayBandBundleStatus.Revoked:
                        return true;
                }
                return false;
            case LenientPayBandBundleStatus.Revoked:
                switch (next)
                {
                    case LenientPayBandBundleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientPayBandBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient pay band bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"lenient pay band bundle"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Revision + (double)ExpiresAt, 2);
    }

    public LenientPayBandBundle Copy()
    {
        return new LenientPayBandBundle(
            Code,
            Label,
            ExpiresAt,
            JobCode,
            Revision,
            EmployeeId,
            Category,
            Phone,
            Version,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
