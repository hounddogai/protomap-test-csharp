namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict recognition policy is in its life cycle.</summary>
public enum StrictRecognitionPolicyStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A strict recognition policy. Its code is unique within a tenant.</summary>
public sealed class StrictRecognitionPolicy(string code, string name, bool visible, string country, int updatedAt, string contactEmail, string notes, double minimum, string checksum, string costCenter, StrictRecognitionPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public string Country { get; set; } = country;
    public int UpdatedAt { get; set; } = updatedAt;
    public string ContactEmail { get; set; } = contactEmail;
    public string Notes { get; set; } = notes;
    public double Minimum { get; set; } = minimum;
    public string Checksum { get; set; } = checksum;
    public string CostCenter { get; set; } = costCenter;
    public StrictRecognitionPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a strict recognition policy in status new.</summary>
    public static StrictRecognitionPolicy NewStrictRecognitionPolicy(string code, string name)
    {
        return new StrictRecognitionPolicy(
            code,
            name,
            false,
            "",
            0,
            "",
            "",
            0.0,
            "",
            "",
            StrictRecognitionPolicyStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictRecognitionPolicyStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictRecognitionPolicyStatus next)
    {
        switch (Status)
        {
            case StrictRecognitionPolicyStatus.New:
                return next == StrictRecognitionPolicyStatus.Done || next == StrictRecognitionPolicyStatus.Failed;
            case StrictRecognitionPolicyStatus.Queued:
                return next == StrictRecognitionPolicyStatus.Running || next == StrictRecognitionPolicyStatus.Done;
            case StrictRecognitionPolicyStatus.Running:
                switch (next)
                {
                    case StrictRecognitionPolicyStatus.Done:
                    case StrictRecognitionPolicyStatus.Failed:
                        return true;
                }
                return false;
            case StrictRecognitionPolicyStatus.Done:
                switch (next)
                {
                    case StrictRecognitionPolicyStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictRecognitionPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict recognition policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Minimum + (double)UpdatedAt, 2);
    }

    public StrictRecognitionPolicy Copy()
    {
        return new StrictRecognitionPolicy(
            Code,
            Name,
            Visible,
            Country,
            UpdatedAt,
            ContactEmail,
            Notes,
            Minimum,
            Checksum,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
