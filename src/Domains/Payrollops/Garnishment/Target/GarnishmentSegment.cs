namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment segment is in its life cycle.</summary>
public enum GarnishmentSegmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A garnishment segment. Its code is unique within a tenant.</summary>
public sealed class GarnishmentSegment(string code, string label, string name, string department, double weight, double rate, double threshold, string costCenter, double minimum, GarnishmentSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public double Weight { get; set; } = weight;
    public double Rate { get; set; } = rate;
    public double Threshold { get; set; } = threshold;
    public string CostCenter { get; set; } = costCenter;
    public double Minimum { get; set; } = minimum;
    public GarnishmentSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment segment in status proposed.</summary>
    public static GarnishmentSegment NewGarnishmentSegment(string code, string label)
    {
        return new GarnishmentSegment(
            code,
            label,
            "",
            "",
            0.0,
            0.0,
            0.0,
            "",
            0.0,
            GarnishmentSegmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentSegmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentSegmentStatus next)
    {
        switch (Status)
        {
            case GarnishmentSegmentStatus.Proposed:
                return next == GarnishmentSegmentStatus.Approved || next == GarnishmentSegmentStatus.Withdrawn;
            case GarnishmentSegmentStatus.Approved:
                switch (next)
                {
                    case GarnishmentSegmentStatus.Rejected:
                    case GarnishmentSegmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case GarnishmentSegmentStatus.Rejected:
                switch (next)
                {
                    case GarnishmentSegmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Minimum * Threshold, 2);
    }

    public GarnishmentSegment Copy()
    {
        return new GarnishmentSegment(
            Code,
            Label,
            Name,
            Department,
            Weight,
            Rate,
            Threshold,
            CostCenter,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
