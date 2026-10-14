namespace Acme.Workforce.Compensation.PayBand.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band window is in its life cycle.</summary>
public enum PayBandWindowStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pay band window. Its code is unique within a tenant.</summary>
public sealed class PayBandWindow(string code, string title, int expiresAt, double budget, string grade, double score, int attempts, string managerId, int sequence, string unit, double rating, PayBandWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Budget { get; set; } = budget;
    public string Grade { get; set; } = grade;
    public double Score { get; set; } = score;
    public int Attempts { get; set; } = attempts;
    public string ManagerId { get; set; } = managerId;
    public int Sequence { get; set; } = sequence;
    public string Unit { get; set; } = unit;
    public double Rating { get; set; } = rating;
    public PayBandWindowStatus Status { get; set; } = status;

    /// <summary>Starts a pay band window in status proposed.</summary>
    public static PayBandWindow NewPayBandWindow(string code, string title)
    {
        return new PayBandWindow(code, title, 0, 0.0, "", 0.0, 0, "", 0, "", 0.0, PayBandWindowStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandWindowStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandWindowStatus next)
    {
        switch (Status)
        {
            case PayBandWindowStatus.Proposed:
                return next == PayBandWindowStatus.Approved || next == PayBandWindowStatus.Rejected;
            case PayBandWindowStatus.Approved:
                switch (next)
                {
                    case PayBandWindowStatus.Rejected:
                        return true;
                }
                return false;
            case PayBandWindowStatus.Rejected:
                return next == PayBandWindowStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PayBandWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Rating * (double)Attempts, 2);
    }

    public PayBandWindow Copy()
    {
        return new PayBandWindow(
            Code,
            Title,
            ExpiresAt,
            Budget,
            Grade,
            Score,
            Attempts,
            ManagerId,
            Sequence,
            Unit,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
