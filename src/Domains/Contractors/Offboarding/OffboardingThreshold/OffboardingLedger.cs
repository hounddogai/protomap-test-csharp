namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding ledger is in its life cycle.</summary>
public enum OffboardingLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A offboarding ledger as the offboarding team keeps it.</summary>
public sealed class OffboardingLedger(string code, string title, string category, Dictionary<string, string> attributes, int tenureMonths, double margin, int revision, int attempts, string salaryBand, double minimum, OffboardingLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Margin { get; set; } = margin;
    public int Revision { get; set; } = revision;
    public int Attempts { get; set; } = attempts;
    public string SalaryBand { get; set; } = salaryBand;
    public double Minimum { get; set; } = minimum;
    public OffboardingLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding ledger in status open.</summary>
    public static OffboardingLedger NewOffboardingLedger(string code, string title)
    {
        return new OffboardingLedger(
            code,
            title,
            "",
            new Dictionary<string, string>(),
            0,
            0.0,
            0,
            0,
            "",
            0.0,
            OffboardingLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingLedgerStatus next)
    {
        switch (Status)
        {
            case OffboardingLedgerStatus.Open:
                return next == OffboardingLedgerStatus.InReview || next == OffboardingLedgerStatus.Archived;
            case OffboardingLedgerStatus.InReview:
                switch (next)
                {
                    case OffboardingLedgerStatus.Resolved:
                        return true;
                }
                return false;
            case OffboardingLedgerStatus.Resolved:
                return next == OffboardingLedgerStatus.Reopened || next == OffboardingLedgerStatus.Archived;
            case OffboardingLedgerStatus.Reopened:
                switch (next)
                {
                    case OffboardingLedgerStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double EffectiveExposure()
    {
        return Math.Round((double)Attempts + (double)Revision, 2);
    }

    public OffboardingLedger Copy()
    {
        return new OffboardingLedger(
            Code,
            Title,
            Category,
            Attributes,
            TenureMonths,
            Margin,
            Revision,
            Attempts,
            SalaryBand,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
