namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy pay run agreement is in its life cycle.</summary>
public enum LegacyPayRunAgreementStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a legacy pay run agreement and its current status.</summary>
public sealed class LegacyPayRunAgreement(string code, string title, double threshold, string team, int graceDays, string reviewer, string badge, double weight, string location, int startsAt, LegacyPayRunAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public string Team { get; set; } = team;
    public int GraceDays { get; set; } = graceDays;
    public string Reviewer { get; set; } = reviewer;
    public string Badge { get; set; } = badge;
    public double Weight { get; set; } = weight;
    public string Location { get; set; } = location;
    public int StartsAt { get; set; } = startsAt;
    public LegacyPayRunAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a legacy pay run agreement in status requested.</summary>
    public static LegacyPayRunAgreement NewLegacyPayRunAgreement(string code, string title)
    {
        return new LegacyPayRunAgreement(
            code,
            title,
            0.0,
            "",
            0,
            "",
            "",
            0.0,
            "",
            0,
            LegacyPayRunAgreementStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPayRunAgreementStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPayRunAgreementStatus next)
    {
        switch (Status)
        {
            case LegacyPayRunAgreementStatus.Requested:
                return next == LegacyPayRunAgreementStatus.Granted;
            case LegacyPayRunAgreementStatus.Granted:
                switch (next)
                {
                    case LegacyPayRunAgreementStatus.Expired:
                        return true;
                }
                return false;
            case LegacyPayRunAgreementStatus.Revoked:
                switch (next)
                {
                    case LegacyPayRunAgreementStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPayRunAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy pay run agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"legacy pay run agreement"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Weight - (double)GraceDays, 2);
    }

    public LegacyPayRunAgreement Copy()
    {
        return new LegacyPayRunAgreement(
            Code,
            Title,
            Threshold,
            Team,
            GraceDays,
            Reviewer,
            Badge,
            Weight,
            Location,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
