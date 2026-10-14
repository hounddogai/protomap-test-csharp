namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled warranty manifest is in its life cycle.</summary>
public enum ScheduledWarrantyManifestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A scheduled warranty manifest. Its code is unique within a tenant.</summary>
public sealed class ScheduledWarrantyManifest(string code, string label, int updatedAt, string description, string grade, double budget, string salaryBand, int sequence, ScheduledWarrantyManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Description { get; set; } = description;
    public string Grade { get; set; } = grade;
    public double Budget { get; set; } = budget;
    public string SalaryBand { get; set; } = salaryBand;
    public int Sequence { get; set; } = sequence;
    public ScheduledWarrantyManifestStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled warranty manifest in status proposed.</summary>
    public static ScheduledWarrantyManifest NewScheduledWarrantyManifest(string code, string label)
    {
        return new ScheduledWarrantyManifest(
            code,
            label,
            0,
            "",
            "",
            0.0,
            "",
            0,
            ScheduledWarrantyManifestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledWarrantyManifestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledWarrantyManifestStatus next)
    {
        switch (Status)
        {
            case ScheduledWarrantyManifestStatus.Proposed:
                switch (next)
                {
                    case ScheduledWarrantyManifestStatus.Rejected:
                    case ScheduledWarrantyManifestStatus.Withdrawn:
                        return true;
                }
                return false;
            case ScheduledWarrantyManifestStatus.Approved:
                switch (next)
                {
                    case ScheduledWarrantyManifestStatus.Rejected:
                    case ScheduledWarrantyManifestStatus.Withdrawn:
                        return true;
                }
                return false;
            case ScheduledWarrantyManifestStatus.Rejected:
                switch (next)
                {
                    case ScheduledWarrantyManifestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledWarrantyManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled warranty manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"scheduled warranty manifest"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)UpdatedAt - Budget, 2);
    }

    public ScheduledWarrantyManifest Copy()
    {
        return new ScheduledWarrantyManifest(
            Code,
            Label,
            UpdatedAt,
            Description,
            Grade,
            Budget,
            SalaryBand,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
