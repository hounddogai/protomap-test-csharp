namespace Acme.Workforce.Workplace.Desk.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global desk manifest is in its life cycle.</summary>
public enum GlobalDeskManifestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A global desk manifest as the desk team keeps it.</summary>
public sealed class GlobalDeskManifest(string code, string title, string channel, int attempts, string salaryBand, string description, int expiresAt, int priority, string department, double margin, double budget, GlobalDeskManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public int Attempts { get; set; } = attempts;
    public string SalaryBand { get; set; } = salaryBand;
    public string Description { get; set; } = description;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Priority { get; set; } = priority;
    public string Department { get; set; } = department;
    public double Margin { get; set; } = margin;
    public double Budget { get; set; } = budget;
    public GlobalDeskManifestStatus Status { get; set; } = status;

    /// <summary>Starts a global desk manifest in status open.</summary>
    public static GlobalDeskManifest NewGlobalDeskManifest(string code, string title)
    {
        return new GlobalDeskManifest(code, title, "", 0, "", "", 0, 0, "", 0.0, 0.0, GlobalDeskManifestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalDeskManifestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalDeskManifestStatus next)
    {
        switch (Status)
        {
            case GlobalDeskManifestStatus.Open:
                return next == GlobalDeskManifestStatus.Resolved || next == GlobalDeskManifestStatus.Reopened;
            case GlobalDeskManifestStatus.InReview:
                switch (next)
                {
                    case GlobalDeskManifestStatus.Resolved:
                        return true;
                }
                return false;
            case GlobalDeskManifestStatus.Resolved:
                return next == GlobalDeskManifestStatus.Archived;
            case GlobalDeskManifestStatus.Reopened:
                switch (next)
                {
                    case GlobalDeskManifestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalDeskManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global desk manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"global desk manifest"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Budget + (double)Attempts, 2);
    }

    public GlobalDeskManifest Copy()
    {
        return new GlobalDeskManifest(
            Code,
            Title,
            Channel,
            Attempts,
            SalaryBand,
            Description,
            ExpiresAt,
            Priority,
            Department,
            Margin,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
