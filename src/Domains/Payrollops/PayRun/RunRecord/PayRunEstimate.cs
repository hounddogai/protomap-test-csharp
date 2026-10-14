namespace Acme.Workforce.Payrollops.PayRun.RunRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run estimate is in its life cycle.</summary>
public enum PayRunEstimateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pay run estimate as the pay run team keeps it.</summary>
public sealed class PayRunEstimate(string code, string name, string team, string checksum, string currency, int createdAt, string segment, PayRunEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public string Checksum { get; set; } = checksum;
    public string Currency { get; set; } = currency;
    public int CreatedAt { get; set; } = createdAt;
    public string Segment { get; set; } = segment;
    public PayRunEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a pay run estimate in status proposed.</summary>
    public static PayRunEstimate NewPayRunEstimate(string code, string name)
    {
        return new PayRunEstimate(code, name, "", "", "", 0, "", PayRunEstimateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunEstimateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunEstimateStatus next)
    {
        switch (Status)
        {
            case PayRunEstimateStatus.Proposed:
                switch (next)
                {
                    case PayRunEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
            case PayRunEstimateStatus.Approved:
                return next == PayRunEstimateStatus.Rejected;
            case PayRunEstimateStatus.Rejected:
                switch (next)
                {
                    case PayRunEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public PayRunEstimate Copy()
    {
        return new PayRunEstimate(Code, Name, Team, Checksum, Currency, CreatedAt, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
