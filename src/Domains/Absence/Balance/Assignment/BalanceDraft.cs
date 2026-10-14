namespace Acme.Workforce.Absence.Balance.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance draft is in its life cycle.</summary>
public enum BalanceDraftStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a balance draft and its current status.</summary>
public sealed class BalanceDraft(string code, string label, int updatedAt, double floorValue, string notes, int retries, string channel, int graceDays, double fte, bool visible, BalanceDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public double FloorValue { get; set; } = floorValue;
    public string Notes { get; set; } = notes;
    public int Retries { get; set; } = retries;
    public string Channel { get; set; } = channel;
    public int GraceDays { get; set; } = graceDays;
    public double Fte { get; set; } = fte;
    public bool Visible { get; set; } = visible;
    public BalanceDraftStatus Status { get; set; } = status;

    /// <summary>Starts a balance draft in status draft.</summary>
    public static BalanceDraft NewBalanceDraft(string code, string label)
    {
        return new BalanceDraft(code, label, 0, 0.0, "", 0, "", 0, 0.0, false, BalanceDraftStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceDraftStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceDraftStatus next)
    {
        switch (Status)
        {
            case BalanceDraftStatus.Draft:
                return next == BalanceDraftStatus.Closed;
            case BalanceDraftStatus.Pending:
                switch (next)
                {
                    case BalanceDraftStatus.Active:
                        return true;
                }
                return false;
            case BalanceDraftStatus.Active:
                return next == BalanceDraftStatus.Closed;
            case BalanceDraftStatus.Suspended:
                switch (next)
                {
                    case BalanceDraftStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)UpdatedAt + Fte, 2);
    }

    public BalanceDraft Copy()
    {
        return new BalanceDraft(
            Code,
            Label,
            UpdatedAt,
            FloorValue,
            Notes,
            Retries,
            Channel,
            GraceDays,
            Fte,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
