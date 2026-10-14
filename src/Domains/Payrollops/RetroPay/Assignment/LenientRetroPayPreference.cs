namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient retro pay preference is in its life cycle.</summary>
public enum LenientRetroPayPreferenceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A lenient retro pay preference. Its code is unique within a tenant.</summary>
public sealed class LenientRetroPayPreference(string code, string title, int sequence, string location, string phone, LenientRetroPayPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Sequence { get; set; } = sequence;
    public string Location { get; set; } = location;
    public string Phone { get; set; } = phone;
    public LenientRetroPayPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a lenient retro pay preference in status idle.</summary>
    public static LenientRetroPayPreference NewLenientRetroPayPreference(string code, string title)
    {
        return new LenientRetroPayPreference(code, title, 0, "", "", LenientRetroPayPreferenceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientRetroPayPreferenceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientRetroPayPreferenceStatus next)
    {
        switch (Status)
        {
            case LenientRetroPayPreferenceStatus.Idle:
                return next == LenientRetroPayPreferenceStatus.Ready || next == LenientRetroPayPreferenceStatus.Stopped;
            case LenientRetroPayPreferenceStatus.Warming:
                return next == LenientRetroPayPreferenceStatus.Draining || next == LenientRetroPayPreferenceStatus.Stopped;
            case LenientRetroPayPreferenceStatus.Ready:
                switch (next)
                {
                    case LenientRetroPayPreferenceStatus.Draining:
                    case LenientRetroPayPreferenceStatus.Stopped:
                        return true;
                }
                return false;
            case LenientRetroPayPreferenceStatus.Draining:
                switch (next)
                {
                    case LenientRetroPayPreferenceStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientRetroPayPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient retro pay preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)Sequence;
    }

    public LenientRetroPayPreference Copy()
    {
        return new LenientRetroPayPreference(Code, Title, Sequence, Location, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
