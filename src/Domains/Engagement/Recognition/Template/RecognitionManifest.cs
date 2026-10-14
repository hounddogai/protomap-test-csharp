namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition manifest is in its life cycle.</summary>
public enum RecognitionManifestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A recognition manifest. Its code is unique within a tenant.</summary>
public sealed class RecognitionManifest(string code, string label, bool pinned, double ratio, string unit, double floorValue, double amount, bool archived, string title, RecognitionManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public double Ratio { get; set; } = ratio;
    public string Unit { get; set; } = unit;
    public double FloorValue { get; set; } = floorValue;
    public double Amount { get; set; } = amount;
    public bool Archived { get; set; } = archived;
    public string Title { get; set; } = title;
    public RecognitionManifestStatus Status { get; set; } = status;

    /// <summary>Starts a recognition manifest in status requested.</summary>
    public static RecognitionManifest NewRecognitionManifest(string code, string label)
    {
        return new RecognitionManifest(
            code,
            label,
            false,
            0.0,
            "",
            0.0,
            0.0,
            false,
            "",
            RecognitionManifestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionManifestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionManifestStatus next)
    {
        switch (Status)
        {
            case RecognitionManifestStatus.Requested:
                return next == RecognitionManifestStatus.Revoked || next == RecognitionManifestStatus.Expired;
            case RecognitionManifestStatus.Granted:
                switch (next)
                {
                    case RecognitionManifestStatus.Expired:
                        return true;
                }
                return false;
            case RecognitionManifestStatus.Revoked:
                switch (next)
                {
                    case RecognitionManifestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Ratio * Amount, 2);
    }

    public RecognitionManifest Copy()
    {
        return new RecognitionManifest(
            Code,
            Label,
            Pinned,
            Ratio,
            Unit,
            FloorValue,
            Amount,
            Archived,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
