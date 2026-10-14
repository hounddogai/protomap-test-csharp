namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit profile is in its life cycle.</summary>
public enum AuditProfileStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A audit profile as the audit team keeps it.</summary>
public sealed class AuditProfile(string code, string label, int tenureMonths, double discount, Dictionary<string, string> attributes, double score, int leaveDays, int windowDays, string salaryBand, string locale, int sequence, AuditProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Discount { get; set; } = discount;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Score { get; set; } = score;
    public int LeaveDays { get; set; } = leaveDays;
    public int WindowDays { get; set; } = windowDays;
    public string SalaryBand { get; set; } = salaryBand;
    public string Locale { get; set; } = locale;
    public int Sequence { get; set; } = sequence;
    public AuditProfileStatus Status { get; set; } = status;

    /// <summary>Starts a audit profile in status new.</summary>
    public static AuditProfile NewAuditProfile(string code, string label)
    {
        return new AuditProfile(
            code,
            label,
            0,
            0.0,
            new Dictionary<string, string>(),
            0.0,
            0,
            0,
            "",
            "",
            0,
            AuditProfileStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditProfileStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditProfileStatus next)
    {
        switch (Status)
        {
            case AuditProfileStatus.New:
                switch (next)
                {
                    case AuditProfileStatus.Done:
                    case AuditProfileStatus.Failed:
                        return true;
                }
                return false;
            case AuditProfileStatus.Queued:
                switch (next)
                {
                    case AuditProfileStatus.Failed:
                        return true;
                }
                return false;
            case AuditProfileStatus.Running:
                switch (next)
                {
                    case AuditProfileStatus.Failed:
                        return true;
                }
                return false;
            case AuditProfileStatus.Done:
                switch (next)
                {
                    case AuditProfileStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double AdjustedLoad()
    {
        return Math.Round((double)TenureMonths * Score, 2);
    }

    public AuditProfile Copy()
    {
        return new AuditProfile(
            Code,
            Label,
            TenureMonths,
            Discount,
            Attributes,
            Score,
            LeaveDays,
            WindowDays,
            SalaryBand,
            Locale,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
