namespace Acme.Workforce.Absence.ReturnPlan.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default return plan manifest is in its life cycle.</summary>
public enum DefaultReturnPlanManifestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A default return plan manifest as the return plan team keeps it.</summary>
public sealed class DefaultReturnPlanManifest(string code, string label, int tenureMonths, string ownerId, string team, double ratio, int batchSize, double score, string name, int graceDays, DefaultReturnPlanManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public string OwnerId { get; set; } = ownerId;
    public string Team { get; set; } = team;
    public double Ratio { get; set; } = ratio;
    public int BatchSize { get; set; } = batchSize;
    public double Score { get; set; } = score;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public DefaultReturnPlanManifestStatus Status { get; set; } = status;

    /// <summary>Starts a default return plan manifest in status proposed.</summary>
    public static DefaultReturnPlanManifest NewDefaultReturnPlanManifest(string code, string label)
    {
        return new DefaultReturnPlanManifest(
            code,
            label,
            0,
            "",
            "",
            0.0,
            0,
            0.0,
            "",
            0,
            DefaultReturnPlanManifestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultReturnPlanManifestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultReturnPlanManifestStatus next)
    {
        switch (Status)
        {
            case DefaultReturnPlanManifestStatus.Proposed:
                switch (next)
                {
                    case DefaultReturnPlanManifestStatus.Approved:
                    case DefaultReturnPlanManifestStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultReturnPlanManifestStatus.Approved:
                return next == DefaultReturnPlanManifestStatus.Withdrawn;
            case DefaultReturnPlanManifestStatus.Rejected:
                switch (next)
                {
                    case DefaultReturnPlanManifestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultReturnPlanManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default return plan manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"default return plan manifest"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(Ratio * (double)GraceDays, 2);
    }

    public DefaultReturnPlanManifest Copy()
    {
        return new DefaultReturnPlanManifest(
            Code,
            Label,
            TenureMonths,
            OwnerId,
            Team,
            Ratio,
            BatchSize,
            Score,
            Name,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
