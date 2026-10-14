namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered engagement agreement is in its life cycle.</summary>
public enum TieredEngagementAgreementStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A tiered engagement agreement as the engagement team keeps it.</summary>
public sealed class TieredEngagementAgreement(string code, string title, int attempts, bool approved, int priority, TieredEngagementAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public bool Approved { get; set; } = approved;
    public int Priority { get; set; } = priority;
    public TieredEngagementAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a tiered engagement agreement in status created.</summary>
    public static TieredEngagementAgreement NewTieredEngagementAgreement(string code, string title)
    {
        return new TieredEngagementAgreement(code, title, 0, false, 0, TieredEngagementAgreementStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredEngagementAgreementStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredEngagementAgreementStatus next)
    {
        switch (Status)
        {
            case TieredEngagementAgreementStatus.Created:
                return next == TieredEngagementAgreementStatus.Retired;
            case TieredEngagementAgreementStatus.Validated:
                switch (next)
                {
                    case TieredEngagementAgreementStatus.Published:
                    case TieredEngagementAgreementStatus.Retired:
                        return true;
                }
                return false;
            case TieredEngagementAgreementStatus.Published:
                return next == TieredEngagementAgreementStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TieredEngagementAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered engagement agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tiered engagement agreement"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Priority + (double)Attempts, 2);
    }

    public TieredEngagementAgreement Copy()
    {
        return new TieredEngagementAgreement(Code, Title, Attempts, Approved, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
