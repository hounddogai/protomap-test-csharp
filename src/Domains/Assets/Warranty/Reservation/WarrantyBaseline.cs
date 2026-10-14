namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty baseline is in its life cycle.</summary>
public enum WarrantyBaselineStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A warranty baseline. Its code is unique within a tenant.</summary>
public sealed class WarrantyBaseline(string code, string label, string segment, string phone, List<string> tags, double score, WarrantyBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public string Phone { get; set; } = phone;
    public List<string> Tags { get; set; } = tags;
    public double Score { get; set; } = score;
    public WarrantyBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a warranty baseline in status new.</summary>
    public static WarrantyBaseline NewWarrantyBaseline(string code, string label)
    {
        return new WarrantyBaseline(code, label, "", "", new List<string>(), 0.0, WarrantyBaselineStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyBaselineStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyBaselineStatus next)
    {
        switch (Status)
        {
            case WarrantyBaselineStatus.New:
                switch (next)
                {
                    case WarrantyBaselineStatus.Queued:
                        return true;
                }
                return false;
            case WarrantyBaselineStatus.Queued:
                return next == WarrantyBaselineStatus.Done || next == WarrantyBaselineStatus.Failed;
            case WarrantyBaselineStatus.Running:
                switch (next)
                {
                    case WarrantyBaselineStatus.Done:
                    case WarrantyBaselineStatus.Failed:
                        return true;
                }
                return false;
            case WarrantyBaselineStatus.Done:
                switch (next)
                {
                    case WarrantyBaselineStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantyBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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
        return Score;
    }

    public WarrantyBaseline Copy()
    {
        return new WarrantyBaseline(Code, Label, Segment, Phone, Tags, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
