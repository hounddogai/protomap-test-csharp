namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive occupancy template is in its life cycle.</summary>
public enum AdaptiveOccupancyTemplateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A adaptive occupancy template. Its code is unique within a tenant.</summary>
public sealed class AdaptiveOccupancyTemplate(string code, string label, string source, double threshold, string costCenter, string department, AdaptiveOccupancyTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public double Threshold { get; set; } = threshold;
    public string CostCenter { get; set; } = costCenter;
    public string Department { get; set; } = department;
    public AdaptiveOccupancyTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive occupancy template in status idle.</summary>
    public static AdaptiveOccupancyTemplate NewAdaptiveOccupancyTemplate(string code, string label)
    {
        return new AdaptiveOccupancyTemplate(code, label, "", 0.0, "", "", AdaptiveOccupancyTemplateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveOccupancyTemplateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveOccupancyTemplateStatus next)
    {
        switch (Status)
        {
            case AdaptiveOccupancyTemplateStatus.Idle:
                return next == AdaptiveOccupancyTemplateStatus.Ready || next == AdaptiveOccupancyTemplateStatus.Stopped;
            case AdaptiveOccupancyTemplateStatus.Warming:
                switch (next)
                {
                    case AdaptiveOccupancyTemplateStatus.Ready:
                    case AdaptiveOccupancyTemplateStatus.Draining:
                        return true;
                }
                return false;
            case AdaptiveOccupancyTemplateStatus.Ready:
                return next == AdaptiveOccupancyTemplateStatus.Draining || next == AdaptiveOccupancyTemplateStatus.Stopped;
            case AdaptiveOccupancyTemplateStatus.Draining:
                switch (next)
                {
                    case AdaptiveOccupancyTemplateStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveOccupancyTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive occupancy template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public AdaptiveOccupancyTemplate Copy()
    {
        return new AdaptiveOccupancyTemplate(Code, Label, Source, Threshold, CostCenter, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
