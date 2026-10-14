namespace Acme.Workforce.Payrollops.Deduction.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction bundle is in its life cycle.</summary>
public enum DeductionBundleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A deduction bundle as the deduction team keeps it.</summary>
public sealed class DeductionBundle(string code, string name, int startsAt, string phone, string notes, string source, List<string> tags, string description, DeductionBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public string Phone { get; set; } = phone;
    public string Notes { get; set; } = notes;
    public string Source { get; set; } = source;
    public List<string> Tags { get; set; } = tags;
    public string Description { get; set; } = description;
    public DeductionBundleStatus Status { get; set; } = status;

    /// <summary>Starts a deduction bundle in status new.</summary>
    public static DeductionBundle NewDeductionBundle(string code, string name)
    {
        return new DeductionBundle(code, name, 0, "", "", "", new List<string>(), "", DeductionBundleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionBundleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionBundleStatus next)
    {
        switch (Status)
        {
            case DeductionBundleStatus.New:
                return next == DeductionBundleStatus.Running;
            case DeductionBundleStatus.Queued:
                return next == DeductionBundleStatus.Failed;
            case DeductionBundleStatus.Running:
                return next == DeductionBundleStatus.Done || next == DeductionBundleStatus.Failed;
            case DeductionBundleStatus.Done:
                return next == DeductionBundleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(DeductionBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"deduction bundle"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public DeductionBundle Copy()
    {
        return new DeductionBundle(Code, Name, StartsAt, Phone, Notes, Source, Tags, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
