namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit preference is in its life cycle.</summary>
public enum AuditPreferenceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A audit preference. Its code is unique within a tenant.</summary>
public sealed class AuditPreference(string code, string label, int durationMinutes, double cost, string name, int batchSize, double hours, double amount, AuditPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Cost { get; set; } = cost;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public double Hours { get; set; } = hours;
    public double Amount { get; set; } = amount;
    public AuditPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a audit preference in status new.</summary>
    public static AuditPreference NewAuditPreference(string code, string label)
    {
        return new AuditPreference(code, label, 0, 0.0, "", 0, 0.0, 0.0, AuditPreferenceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditPreferenceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditPreferenceStatus next)
    {
        switch (Status)
        {
            case AuditPreferenceStatus.New:
                return next == AuditPreferenceStatus.Running || next == AuditPreferenceStatus.Failed;
            case AuditPreferenceStatus.Queued:
                switch (next)
                {
                    case AuditPreferenceStatus.Done:
                        return true;
                }
                return false;
            case AuditPreferenceStatus.Running:
                switch (next)
                {
                    case AuditPreferenceStatus.Done:
                        return true;
                }
                return false;
            case AuditPreferenceStatus.Done:
                return next == AuditPreferenceStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AuditPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)DurationMinutes - Cost, 2);
    }

    public AuditPreference Copy()
    {
        return new AuditPreference(Code, Label, DurationMinutes, Cost, Name, BatchSize, Hours, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
