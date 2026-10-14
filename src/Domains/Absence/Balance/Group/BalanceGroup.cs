namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance group is in its life cycle.</summary>
public enum BalanceGroupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A balance group as the balance team keeps it.</summary>
public sealed class BalanceGroup(string code, string label, string employeeId, string notes, int sequence, string locale, string reviewer, bool pinned, string location, string phone, BalanceGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public string Notes { get; set; } = notes;
    public int Sequence { get; set; } = sequence;
    public string Locale { get; set; } = locale;
    public string Reviewer { get; set; } = reviewer;
    public bool Pinned { get; set; } = pinned;
    public string Location { get; set; } = location;
    public string Phone { get; set; } = phone;
    public BalanceGroupStatus Status { get; set; } = status;

    /// <summary>Starts a balance group in status open.</summary>
    public static BalanceGroup NewBalanceGroup(string code, string label)
    {
        return new BalanceGroup(code, label, "", "", 0, "", "", false, "", "", BalanceGroupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceGroupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceGroupStatus next)
    {
        switch (Status)
        {
            case BalanceGroupStatus.Open:
                return next == BalanceGroupStatus.Reopened || next == BalanceGroupStatus.Archived;
            case BalanceGroupStatus.InReview:
                return next == BalanceGroupStatus.Archived;
            case BalanceGroupStatus.Resolved:
                return next == BalanceGroupStatus.Archived;
            case BalanceGroupStatus.Reopened:
                return next == BalanceGroupStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BalanceGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)Sequence;
    }

    public BalanceGroup Copy()
    {
        return new BalanceGroup(
            Code,
            Label,
            EmployeeId,
            Notes,
            Sequence,
            Locale,
            Reviewer,
            Pinned,
            Location,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
