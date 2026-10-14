namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual action plan record is in its life cycle.</summary>
public enum ManualActionPlanRecordStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A manual action plan record as the action plan team keeps it.</summary>
public sealed class ManualActionPlanRecord(string code, string title, int version, string category, double discount, double hours, string name, int expiresAt, int leaveDays, ManualActionPlanRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public string Category { get; set; } = category;
    public double Discount { get; set; } = discount;
    public double Hours { get; set; } = hours;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public int LeaveDays { get; set; } = leaveDays;
    public ManualActionPlanRecordStatus Status { get; set; } = status;

    /// <summary>Starts a manual action plan record in status requested.</summary>
    public static ManualActionPlanRecord NewManualActionPlanRecord(string code, string title)
    {
        return new ManualActionPlanRecord(
            code,
            title,
            0,
            "",
            0.0,
            0.0,
            "",
            0,
            0,
            ManualActionPlanRecordStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualActionPlanRecordStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualActionPlanRecordStatus next)
    {
        switch (Status)
        {
            case ManualActionPlanRecordStatus.Requested:
                switch (next)
                {
                    case ManualActionPlanRecordStatus.Granted:
                    case ManualActionPlanRecordStatus.Expired:
                        return true;
                }
                return false;
            case ManualActionPlanRecordStatus.Granted:
                return next == ManualActionPlanRecordStatus.Expired;
            case ManualActionPlanRecordStatus.Revoked:
                return next == ManualActionPlanRecordStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ManualActionPlanRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual action plan record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Discount * Hours, 2);
    }

    public ManualActionPlanRecord Copy()
    {
        return new ManualActionPlanRecord(
            Code,
            Title,
            Version,
            Category,
            Discount,
            Hours,
            Name,
            ExpiresAt,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
