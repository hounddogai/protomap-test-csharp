namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive warranty audit is in its life cycle.</summary>
public enum AdaptiveWarrantyAuditStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A adaptive warranty audit as the warranty team keeps it.</summary>
public sealed class AdaptiveWarrantyAudit(string code, string label, string locale, string jobCode, string badge, bool approved, double amount, AdaptiveWarrantyAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public string JobCode { get; set; } = jobCode;
    public string Badge { get; set; } = badge;
    public bool Approved { get; set; } = approved;
    public double Amount { get; set; } = amount;
    public AdaptiveWarrantyAuditStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive warranty audit in status requested.</summary>
    public static AdaptiveWarrantyAudit NewAdaptiveWarrantyAudit(string code, string label)
    {
        return new AdaptiveWarrantyAudit(code, label, "", "", "", false, 0.0, AdaptiveWarrantyAuditStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveWarrantyAuditStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveWarrantyAuditStatus next)
    {
        switch (Status)
        {
            case AdaptiveWarrantyAuditStatus.Requested:
                switch (next)
                {
                    case AdaptiveWarrantyAuditStatus.Granted:
                        return true;
                }
                return false;
            case AdaptiveWarrantyAuditStatus.Granted:
                switch (next)
                {
                    case AdaptiveWarrantyAuditStatus.Revoked:
                    case AdaptiveWarrantyAuditStatus.Expired:
                        return true;
                }
                return false;
            case AdaptiveWarrantyAuditStatus.Revoked:
                switch (next)
                {
                    case AdaptiveWarrantyAuditStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveWarrantyAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive warranty audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public AdaptiveWarrantyAudit Copy()
    {
        return new AdaptiveWarrantyAudit(Code, Label, Locale, JobCode, Badge, Approved, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
