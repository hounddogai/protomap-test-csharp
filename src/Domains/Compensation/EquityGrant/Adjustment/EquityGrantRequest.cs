namespace Acme.Workforce.Compensation.EquityGrant.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant request is in its life cycle.</summary>
public enum EquityGrantRequestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a equity grant request and its current status.</summary>
public sealed class EquityGrantRequest(string code, string title, string name, double amount, int startsAt, string notes, string costCenter, int attempts, double hours, EquityGrantRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public int StartsAt { get; set; } = startsAt;
    public string Notes { get; set; } = notes;
    public string CostCenter { get; set; } = costCenter;
    public int Attempts { get; set; } = attempts;
    public double Hours { get; set; } = hours;
    public EquityGrantRequestStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant request in status draft.</summary>
    public static EquityGrantRequest NewEquityGrantRequest(string code, string title)
    {
        return new EquityGrantRequest(code, title, "", 0.0, 0, "", "", 0, 0.0, EquityGrantRequestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantRequestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantRequestStatus next)
    {
        switch (Status)
        {
            case EquityGrantRequestStatus.Draft:
                switch (next)
                {
                    case EquityGrantRequestStatus.Pending:
                    case EquityGrantRequestStatus.Active:
                        return true;
                }
                return false;
            case EquityGrantRequestStatus.Pending:
                switch (next)
                {
                    case EquityGrantRequestStatus.Active:
                    case EquityGrantRequestStatus.Closed:
                        return true;
                }
                return false;
            case EquityGrantRequestStatus.Active:
                return next == EquityGrantRequestStatus.Closed;
            case EquityGrantRequestStatus.Suspended:
                switch (next)
                {
                    case EquityGrantRequestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Amount * (double)Attempts, 2);
    }

    public EquityGrantRequest Copy()
    {
        return new EquityGrantRequest(
            Code,
            Title,
            Name,
            Amount,
            StartsAt,
            Notes,
            CostCenter,
            Attempts,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
