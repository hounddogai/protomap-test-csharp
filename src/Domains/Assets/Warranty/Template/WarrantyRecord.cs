namespace Acme.Workforce.Assets.Warranty.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty record is in its life cycle.</summary>
public enum WarrantyRecordStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A warranty record as the warranty team keeps it.</summary>
public sealed class WarrantyRecord(string code, string title, string locale, double weight, bool pinned, string contactEmail, double ceiling, string notes, double margin, WarrantyRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public double Weight { get; set; } = weight;
    public bool Pinned { get; set; } = pinned;
    public string ContactEmail { get; set; } = contactEmail;
    public double Ceiling { get; set; } = ceiling;
    public string Notes { get; set; } = notes;
    public double Margin { get; set; } = margin;
    public WarrantyRecordStatus Status { get; set; } = status;

    /// <summary>Starts a warranty record in status idle.</summary>
    public static WarrantyRecord NewWarrantyRecord(string code, string title)
    {
        return new WarrantyRecord(code, title, "", 0.0, false, "", 0.0, "", 0.0, WarrantyRecordStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyRecordStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyRecordStatus next)
    {
        switch (Status)
        {
            case WarrantyRecordStatus.Idle:
                return next == WarrantyRecordStatus.Warming || next == WarrantyRecordStatus.Ready;
            case WarrantyRecordStatus.Warming:
                return next == WarrantyRecordStatus.Ready || next == WarrantyRecordStatus.Stopped;
            case WarrantyRecordStatus.Ready:
                return next == WarrantyRecordStatus.Draining || next == WarrantyRecordStatus.Stopped;
            case WarrantyRecordStatus.Draining:
                return next == WarrantyRecordStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(WarrantyRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"warranty record"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Ceiling + Weight, 2);
    }

    public WarrantyRecord Copy()
    {
        return new WarrantyRecord(Code, Title, Locale, Weight, Pinned, ContactEmail, Ceiling, Notes, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
