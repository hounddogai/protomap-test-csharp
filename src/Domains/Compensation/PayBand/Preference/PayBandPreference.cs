namespace Acme.Workforce.Compensation.PayBand.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band preference is in its life cycle.</summary>
public enum PayBandPreferenceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pay band preference as the pay band team keeps it.</summary>
public sealed class PayBandPreference(string code, string title, string badge, bool active, int updatedAt, PayBandPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public bool Active { get; set; } = active;
    public int UpdatedAt { get; set; } = updatedAt;
    public PayBandPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a pay band preference in status new.</summary>
    public static PayBandPreference NewPayBandPreference(string code, string title)
    {
        return new PayBandPreference(code, title, "", false, 0, PayBandPreferenceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandPreferenceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandPreferenceStatus next)
    {
        switch (Status)
        {
            case PayBandPreferenceStatus.New:
                return next == PayBandPreferenceStatus.Queued;
            case PayBandPreferenceStatus.Queued:
                switch (next)
                {
                    case PayBandPreferenceStatus.Failed:
                        return true;
                }
                return false;
            case PayBandPreferenceStatus.Running:
                return next == PayBandPreferenceStatus.Done || next == PayBandPreferenceStatus.Failed;
            case PayBandPreferenceStatus.Done:
                return next == PayBandPreferenceStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PayBandPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)UpdatedAt;
    }

    public PayBandPreference Copy()
    {
        return new PayBandPreference(Code, Title, Badge, Active, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
