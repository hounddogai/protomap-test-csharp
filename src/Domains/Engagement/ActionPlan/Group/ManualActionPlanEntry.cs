namespace Acme.Workforce.Engagement.ActionPlan.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual action plan entry is in its life cycle.</summary>
public enum ManualActionPlanEntryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual action plan entry. Its code is unique within a tenant.</summary>
public sealed class ManualActionPlanEntry(string code, string label, int updatedAt, int durationMinutes, string externalRef, string phone, double weight, string country, string title, int windowDays, ManualActionPlanEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ExternalRef { get; set; } = externalRef;
    public string Phone { get; set; } = phone;
    public double Weight { get; set; } = weight;
    public string Country { get; set; } = country;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public ManualActionPlanEntryStatus Status { get; set; } = status;

    /// <summary>Starts a manual action plan entry in status created.</summary>
    public static ManualActionPlanEntry NewManualActionPlanEntry(string code, string label)
    {
        return new ManualActionPlanEntry(
            code,
            label,
            0,
            0,
            "",
            "",
            0.0,
            "",
            "",
            0,
            ManualActionPlanEntryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualActionPlanEntryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualActionPlanEntryStatus next)
    {
        switch (Status)
        {
            case ManualActionPlanEntryStatus.Created:
                return next == ManualActionPlanEntryStatus.Published || next == ManualActionPlanEntryStatus.Retired;
            case ManualActionPlanEntryStatus.Validated:
                switch (next)
                {
                    case ManualActionPlanEntryStatus.Published:
                        return true;
                }
                return false;
            case ManualActionPlanEntryStatus.Published:
                switch (next)
                {
                    case ManualActionPlanEntryStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualActionPlanEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual action plan entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetLoad()
    {
        return Math.Round(Weight + (double)WindowDays, 2);
    }

    public ManualActionPlanEntry Copy()
    {
        return new ManualActionPlanEntry(
            Code,
            Label,
            UpdatedAt,
            DurationMinutes,
            ExternalRef,
            Phone,
            Weight,
            Country,
            Title,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
