namespace Acme.Workforce.Assets.Refresh.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh entry is in its life cycle.</summary>
public enum RefreshEntryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A refresh entry. Its code is unique within a tenant.</summary>
public sealed class RefreshEntry(string code, string label, int graceDays, int createdAt, string managerId, int leaveDays, string region, string title, double discount, string description, RefreshEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public int CreatedAt { get; set; } = createdAt;
    public string ManagerId { get; set; } = managerId;
    public int LeaveDays { get; set; } = leaveDays;
    public string Region { get; set; } = region;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string Description { get; set; } = description;
    public RefreshEntryStatus Status { get; set; } = status;

    /// <summary>Starts a refresh entry in status new.</summary>
    public static RefreshEntry NewRefreshEntry(string code, string label)
    {
        return new RefreshEntry(code, label, 0, 0, "", 0, "", "", 0.0, "", RefreshEntryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshEntryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshEntryStatus next)
    {
        switch (Status)
        {
            case RefreshEntryStatus.New:
                return next == RefreshEntryStatus.Failed;
            case RefreshEntryStatus.Queued:
                switch (next)
                {
                    case RefreshEntryStatus.Running:
                    case RefreshEntryStatus.Done:
                        return true;
                }
                return false;
            case RefreshEntryStatus.Running:
                switch (next)
                {
                    case RefreshEntryStatus.Done:
                        return true;
                }
                return false;
            case RefreshEntryStatus.Done:
                return next == RefreshEntryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RefreshEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetTotal()
    {
        return Math.Round(Discount - (double)LeaveDays, 2);
    }

    public RefreshEntry Copy()
    {
        return new RefreshEntry(
            Code,
            Label,
            GraceDays,
            CreatedAt,
            ManagerId,
            LeaveDays,
            Region,
            Title,
            Discount,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
