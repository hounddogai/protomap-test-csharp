namespace Acme.Workforce.Compensation.EquityGrant.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant adjustment is in its life cycle.</summary>
public enum EquityGrantAdjustmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A equity grant adjustment as the equity grant team keeps it.</summary>
public sealed class EquityGrantAdjustment(string code, string title, double maximum, string grade, string phone, int sequence, int startsAt, string externalRef, bool active, string summary, EquityGrantAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public string Grade { get; set; } = grade;
    public string Phone { get; set; } = phone;
    public int Sequence { get; set; } = sequence;
    public int StartsAt { get; set; } = startsAt;
    public string ExternalRef { get; set; } = externalRef;
    public bool Active { get; set; } = active;
    public string Summary { get; set; } = summary;
    public EquityGrantAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant adjustment in status draft.</summary>
    public static EquityGrantAdjustment NewEquityGrantAdjustment(string code, string title)
    {
        return new EquityGrantAdjustment(
            code,
            title,
            0.0,
            "",
            "",
            0,
            0,
            "",
            false,
            "",
            EquityGrantAdjustmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantAdjustmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantAdjustmentStatus next)
    {
        switch (Status)
        {
            case EquityGrantAdjustmentStatus.Draft:
                switch (next)
                {
                    case EquityGrantAdjustmentStatus.Pending:
                    case EquityGrantAdjustmentStatus.Suspended:
                        return true;
                }
                return false;
            case EquityGrantAdjustmentStatus.Pending:
                switch (next)
                {
                    case EquityGrantAdjustmentStatus.Suspended:
                    case EquityGrantAdjustmentStatus.Closed:
                        return true;
                }
                return false;
            case EquityGrantAdjustmentStatus.Active:
                switch (next)
                {
                    case EquityGrantAdjustmentStatus.Suspended:
                        return true;
                }
                return false;
            case EquityGrantAdjustmentStatus.Suspended:
                return next == EquityGrantAdjustmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(EquityGrantAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)StartsAt * Maximum, 2);
    }

    public EquityGrantAdjustment Copy()
    {
        return new EquityGrantAdjustment(
            Code,
            Title,
            Maximum,
            Grade,
            Phone,
            Sequence,
            StartsAt,
            ExternalRef,
            Active,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
