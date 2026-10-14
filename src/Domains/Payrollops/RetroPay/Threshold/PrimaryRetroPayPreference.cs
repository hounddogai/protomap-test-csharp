namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary retro pay preference is in its life cycle.</summary>
public enum PrimaryRetroPayPreferenceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a primary retro pay preference and its current status.</summary>
public sealed class PrimaryRetroPayPreference(string code, string name, int version, string badge, string employeeId, string notes, int capacity, string title, int endsAt, int batchSize, bool visible, PrimaryRetroPayPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public string Badge { get; set; } = badge;
    public string EmployeeId { get; set; } = employeeId;
    public string Notes { get; set; } = notes;
    public int Capacity { get; set; } = capacity;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public int BatchSize { get; set; } = batchSize;
    public bool Visible { get; set; } = visible;
    public PrimaryRetroPayPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a primary retro pay preference in status open.</summary>
    public static PrimaryRetroPayPreference NewPrimaryRetroPayPreference(string code, string name)
    {
        return new PrimaryRetroPayPreference(
            code,
            name,
            0,
            "",
            "",
            "",
            0,
            "",
            0,
            0,
            false,
            PrimaryRetroPayPreferenceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryRetroPayPreferenceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryRetroPayPreferenceStatus next)
    {
        switch (Status)
        {
            case PrimaryRetroPayPreferenceStatus.Open:
                switch (next)
                {
                    case PrimaryRetroPayPreferenceStatus.InReview:
                    case PrimaryRetroPayPreferenceStatus.Reopened:
                        return true;
                }
                return false;
            case PrimaryRetroPayPreferenceStatus.InReview:
                return next == PrimaryRetroPayPreferenceStatus.Resolved;
            case PrimaryRetroPayPreferenceStatus.Resolved:
                switch (next)
                {
                    case PrimaryRetroPayPreferenceStatus.Reopened:
                        return true;
                }
                return false;
            case PrimaryRetroPayPreferenceStatus.Reopened:
                switch (next)
                {
                    case PrimaryRetroPayPreferenceStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryRetroPayPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary retro pay preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetLoad()
    {
        return Math.Round((double)Version * (double)Capacity, 2);
    }

    public PrimaryRetroPayPreference Copy()
    {
        return new PrimaryRetroPayPreference(
            Code,
            Name,
            Version,
            Badge,
            EmployeeId,
            Notes,
            Capacity,
            Title,
            EndsAt,
            BatchSize,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
