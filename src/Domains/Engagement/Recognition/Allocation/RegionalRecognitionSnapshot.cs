namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional recognition snapshot is in its life cycle.</summary>
public enum RegionalRecognitionSnapshotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A regional recognition snapshot. Its code is unique within a tenant.</summary>
public sealed class RegionalRecognitionSnapshot(string code, string title, double budget, double amount, string category, string employeeId, string jobCode, string phone, RegionalRecognitionSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public double Amount { get; set; } = amount;
    public string Category { get; set; } = category;
    public string EmployeeId { get; set; } = employeeId;
    public string JobCode { get; set; } = jobCode;
    public string Phone { get; set; } = phone;
    public RegionalRecognitionSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a regional recognition snapshot in status open.</summary>
    public static RegionalRecognitionSnapshot NewRegionalRecognitionSnapshot(string code, string title)
    {
        return new RegionalRecognitionSnapshot(
            code,
            title,
            0.0,
            0.0,
            "",
            "",
            "",
            "",
            RegionalRecognitionSnapshotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalRecognitionSnapshotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalRecognitionSnapshotStatus next)
    {
        switch (Status)
        {
            case RegionalRecognitionSnapshotStatus.Open:
                switch (next)
                {
                    case RegionalRecognitionSnapshotStatus.Archived:
                        return true;
                }
                return false;
            case RegionalRecognitionSnapshotStatus.InReview:
                switch (next)
                {
                    case RegionalRecognitionSnapshotStatus.Resolved:
                    case RegionalRecognitionSnapshotStatus.Archived:
                        return true;
                }
                return false;
            case RegionalRecognitionSnapshotStatus.Resolved:
                return next == RegionalRecognitionSnapshotStatus.Reopened || next == RegionalRecognitionSnapshotStatus.Archived;
            case RegionalRecognitionSnapshotStatus.Reopened:
                return next == RegionalRecognitionSnapshotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RegionalRecognitionSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional recognition snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetTotal()
    {
        return Math.Round(Amount * Budget, 2);
    }

    public RegionalRecognitionSnapshot Copy()
    {
        return new RegionalRecognitionSnapshot(
            Code,
            Title,
            Budget,
            Amount,
            Category,
            EmployeeId,
            JobCode,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
