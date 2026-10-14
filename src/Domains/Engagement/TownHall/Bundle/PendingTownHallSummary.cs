namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending town hall summary is in its life cycle.</summary>
public enum PendingTownHallSummaryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pending town hall summary. Its code is unique within a tenant.</summary>
public sealed class PendingTownHallSummary(string code, string title, int quantity, string costCenter, string employeeId, int startsAt, int graceDays, int batchSize, int tenureMonths, bool active, PendingTownHallSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public string CostCenter { get; set; } = costCenter;
    public string EmployeeId { get; set; } = employeeId;
    public int StartsAt { get; set; } = startsAt;
    public int GraceDays { get; set; } = graceDays;
    public int BatchSize { get; set; } = batchSize;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Active { get; set; } = active;
    public PendingTownHallSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a pending town hall summary in status proposed.</summary>
    public static PendingTownHallSummary NewPendingTownHallSummary(string code, string title)
    {
        return new PendingTownHallSummary(
            code,
            title,
            0,
            "",
            "",
            0,
            0,
            0,
            0,
            false,
            PendingTownHallSummaryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingTownHallSummaryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingTownHallSummaryStatus next)
    {
        switch (Status)
        {
            case PendingTownHallSummaryStatus.Proposed:
                switch (next)
                {
                    case PendingTownHallSummaryStatus.Rejected:
                        return true;
                }
                return false;
            case PendingTownHallSummaryStatus.Approved:
                switch (next)
                {
                    case PendingTownHallSummaryStatus.Rejected:
                    case PendingTownHallSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
            case PendingTownHallSummaryStatus.Rejected:
                return next == PendingTownHallSummaryStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PendingTownHallSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending town hall summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)StartsAt + (double)TenureMonths, 2);
    }

    public PendingTownHallSummary Copy()
    {
        return new PendingTownHallSummary(
            Code,
            Title,
            Quantity,
            CostCenter,
            EmployeeId,
            StartsAt,
            GraceDays,
            BatchSize,
            TenureMonths,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
